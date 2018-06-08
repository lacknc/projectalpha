using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace MissingData
{
    public class MissingDataSim : ActionAdapterBase
    {
        public override bool SupportsTemporalProcessing => throw new NotImplementedException();

        protected override void PublishFrame(IFrame frame, int index)
        {
            throw new NotImplementedException();
        }
    }
}
