using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace MissingData
{
    [Description("MissingDataSimulator: This generates Missing Data For Testing Algortihms")]
    public class MissingDataSim : ActionAdapterBase
    {
        public override bool SupportsTemporalProcessing
        {
            get { return false; }
        }

        #region[Private]

        private double RandomDropProbability;
        private int MissingPointsGenerated;
        private int TotalPointsGenerated;
        private bool UniformDrop;

        private Random RandomGenerator;

        #endregion[Private]

        #region[Parameters]


        [ConnectionStringParameter, Description("Uniform Data Drop"), DefaultValue(false)]
        public bool DropUniform
        {
            get
            {
                return UniformDrop;
            }
            set
            {
                UniformDrop = value;
            }
        }

        [ConnectionStringParameter, Description("Probability of Uniform Data Drop"), DefaultValue(0.0)]
        public double Puniform
        {
            get
            {
                return (RandomDropProbability*100);
            }
            set
            {
                RandomDropProbability = value/100;
            }
        }

        #endregion[Parameters]

        // #### Initalization ####
        public override void Initialize()
        {
            base.Initialize();

            #region[getSettings]

            Dictionary<string, string> settings = Settings;
            string setting;

            //Uniform Data Drop

            if (settings.TryGetValue("DropUniform", out setting))
            {
                UniformDrop = Convert.ToBoolean(setting);
            }
            else
            {
                UniformDrop = false;
            }

            if (settings.TryGetValue("Puniform", out setting))
            {
                RandomDropProbability = Convert.ToDouble(setting)/100.0;
            }
            else
            {
                RandomDropProbability = 0;
            }

            #endregion[getSettings]

            //Ensure Input and Output have the same length
            int nIn = this.InputMeasurementKeys.Length;
            int nOut = this.OutputMeasurements.Length;

            if (nIn != nOut)
            {
                StringBuilder bld_msg = new StringBuilder();
                bld_msg.AppendFormat("There are {0} inputs but {1} outputs", nIn, nOut);
                string msg = bld_msg.ToString();
                OnStatusMessage(GSF.Diagnostics.MessageLevel.Warning, msg);

                

                if (nIn > nOut)
                {
                    MeasurementKey[] input = new MeasurementKey[nOut];
                    IMeasurement[] output = new IMeasurement[nOut];

                    output = this.OutputMeasurements;
                    for (int i=0; i < nOut; i++)
                    {
                        input[i] = this.InputMeasurementKeys[i];
                    }

                    this.OutputMeasurements = output;
                    this.InputMeasurementKeys = input;
                }
                else
                {
                    MeasurementKey[] input = new MeasurementKey[nIn];
                    IMeasurement[] output = new IMeasurement[nIn];

                    input = this.InputMeasurementKeys;
                    for (int i = 0; i < nIn; i++)
                    {
                        output[i] = this.OutputMeasurements[i];
                    }

                    this.OutputMeasurements = output;
                    this.InputMeasurementKeys = input;
                }

                

            }

            // Set up Random Number generator
            RandomGenerator = new Random();

            ResetCounter();


        }
        protected override void PublishFrame(IFrame frame, int index)
        {
            ConcurrentDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;
            IMeasurement[] outputMeasurements = new IMeasurement[OutputMeasurements.Length];

            if (index==0)
            {
                double Percentage;
                if (TotalPointsGenerated > 0)
                {
                    Percentage = MissingPointsGenerated / TotalPointsGenerated;
                }
                else
                {
                    Percentage = 0;
                }
                Percentage = Percentage * 100.0;

                StringBuilder bld_msg = new StringBuilder();
                bld_msg.AppendFormat("Generated {0} missing points ({1}%)", MissingPointsGenerated, Percentage);
                string msg = bld_msg.ToString();
                OnStatusMessage(GSF.Diagnostics.MessageLevel.Info,msg);
                ResetCounter();

            }
            
            for (int i = 0; i < InputMeasurementKeys.Length; i++)
            {
                IMeasurement measurement;
                double value = 0.0;

                if (measurements.TryGetValue(InputMeasurementKeys[i], out measurement) && measurement.ValueQualityIsGood())
                {
                    value = measurement.AdjustedValue;
                }


                if (UniformDrop)
                {
                    double r = RandomGenerator.NextDouble();

                    if (r < RandomDropProbability)
                    {
                        value = Double.NaN;
                        MissingPointsGenerated++;
                    }
                }

                TotalPointsGenerated++;

                Measurement output = Measurement.Clone(OutputMeasurements[i], value, frame.Timestamp);
                outputMeasurements[i] = output;

            }

            OnNewMeasurements(outputMeasurements);

        }

        private void ResetCounter()
        {
            MissingPointsGenerated = 0;
            TotalPointsGenerated = 0;
        }
    }
}
