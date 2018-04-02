using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using CSCore;
using CSCore.DSP;

namespace LedMatrix.Client.Audio
{
   public class LineSpectrum
   {
      private const double MinDbValue = -90;
      private const double MaxDbValue = 0;
      private const double DbScale = (MaxDbValue - MinDbValue);
      private int _fftSize;

      private int _maxFftIndex;
      private int _maximumFrequency = 14000;
      private int _minimumFrequency = 20;
      private int _maximumFrequencyIndex;
      private int _minimumFrequencyIndex;
      private int[] _spectrumIndexMax;
      private int[] _spectrumLogScaleIndexMax;
      private SpectrumProvider _spectrumProvider;

      private ScalingStrategy _scalingStrategy;
      private const int ScaleFactorLinear = 10;
      private const int ScaleFactorSquareRoot = 4;

      public LineSpectrum(SpectrumProvider provider, int barCount)
      {
         SetFftSize(provider.FftSize);
         _spectrumProvider = provider;
         _scalingStrategy = ScalingStrategy.SquareRoot;

         UpdateFrequencyMapping(barCount);
      }

      private void SetFftSize(FftSize size)
      {
         if ((int)Math.Log((int)size, 2) % 1 != 0)
            throw new ArgumentOutOfRangeException("value");

         _fftSize = (int)size;
         _maxFftIndex = _fftSize / 2 - 1;
      }

      private void UpdateFrequencyMapping(int spectrumResolution)
      {
         _maximumFrequencyIndex = Math.Min(_spectrumProvider.GetFftBandIndex(_maximumFrequency) + 1, _maxFftIndex);
         _minimumFrequencyIndex = Math.Min(_spectrumProvider.GetFftBandIndex(_minimumFrequency), _maxFftIndex);

         int indexCount = _maximumFrequencyIndex - _minimumFrequencyIndex;
         double linearIndexBucketSize = Math.Round(indexCount / (double)spectrumResolution, 3);

         _spectrumIndexMax = _spectrumIndexMax.CheckBuffer(spectrumResolution, true);
         _spectrumLogScaleIndexMax = _spectrumLogScaleIndexMax.CheckBuffer(spectrumResolution, true);

         double maxLog = Math.Log(spectrumResolution, spectrumResolution);
         for (int i = 1; i < spectrumResolution; i++)
         {
            int logIndex =
                (int)((maxLog - Math.Log((spectrumResolution + 1) - i, (spectrumResolution + 1))) * indexCount) +
                _minimumFrequencyIndex;

            _spectrumIndexMax[i - 1] = _minimumFrequencyIndex + (int)(i * linearIndexBucketSize);
            _spectrumLogScaleIndexMax[i - 1] = logIndex;
         }

         if (spectrumResolution > 0)
         {
            _spectrumIndexMax[_spectrumIndexMax.Length - 1] =
                _spectrumLogScaleIndexMax[_spectrumLogScaleIndexMax.Length - 1] = _maximumFrequencyIndex;
         }
      }

      protected virtual SpectrumPointData[] CalculateSpectrumPoints(double maxValue, float[] fftBuffer)
      {
         var dataPoints = new List<SpectrumPointData>();

         double value0 = 0, value = 0;
         double lastValue = 0;
         double actualMaxValue = maxValue;
         int spectrumPointIndex = 0;

         for (int i = _minimumFrequencyIndex; i <= _maximumFrequencyIndex; i++)
         {
            switch (_scalingStrategy)
            {
               case ScalingStrategy.Decibel:
                  value0 = (((20 * Math.Log10(fftBuffer[i])) - MinDbValue) / DbScale) * actualMaxValue;
                  break;
               case ScalingStrategy.Linear:
                  value0 = (fftBuffer[i] * ScaleFactorLinear) * actualMaxValue;
                  break;
               case ScalingStrategy.SquareRoot:
                  value0 = ((Math.Sqrt(fftBuffer[i])) * ScaleFactorSquareRoot) * actualMaxValue;
                  break;
            }

            bool recalc = true;

            value = Math.Max(0, Math.Max(value0, value));

            while (spectrumPointIndex <= _spectrumIndexMax.Length - 1 &&
                   i == _spectrumLogScaleIndexMax[spectrumPointIndex])
            {
               if (!recalc)
                  value = lastValue;

               if (value > maxValue)
                  value = maxValue;

               if (spectrumPointIndex > 0)
                  value = (lastValue + value) / 2.0;

               dataPoints.Add(new SpectrumPointData { SpectrumPointIndex = spectrumPointIndex, Value = value });

               lastValue = value;
               value = 0.0;
               spectrumPointIndex++;
               recalc = false;
            }
         }

         return dataPoints.ToArray();
      }

      protected struct SpectrumPointData
      {
         public int SpectrumPointIndex;
         public double Value;
      }

      public byte[] CreateSpectrumGraph()
      {
         var fftBuffer = new float[_fftSize];

         //get the fft result from the spectrum provider
         if (_spectrumProvider.GetFftData(fftBuffer, this))
         {
            double maxHeight = 1024.0;
            SpectrumPointData[] spectrumPoints = CalculateSpectrumPoints(maxHeight, fftBuffer);

            Func<double, byte> numLights = (d) =>
             {
                int n = (d >= 0.900 ? 0x01 : 0x00) |
                        (d >= 0.775 ? 0x02 : 0x00) |
                        (d >= 0.650 ? 0x04 : 0x00) |
                        (d >= 0.525 ? 0x08 : 0x00) |
                        (d >= 0.400 ? 0x10 : 0x00) |
                        (d >= 0.275 ? 0x20 : 0x00) |
                        (d >= 0.150 ? 0x40 : 0x00) |
                        (d >= 0.025 ? 0x80 : 0x00);

                return (byte)(n & 0xFF);
             };

            return spectrumPoints.Select(c => numLights(c.Value/maxHeight)).ToArray();
         }

         return null;
      }
   }
}