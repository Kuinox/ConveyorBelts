using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuinox.ConveyorBelts
{
    public interface IBeltExtractor
    {
        public void InsertItem( long currentTime );
    }
}
