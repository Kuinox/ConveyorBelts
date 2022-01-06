using Kuinox.ConveyorBelts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kuinox.ConveyorBelts
{
    public class Belt : IBeltExtractor
    {
        readonly List<long> _items = new( new long[] { 0u } );
        readonly Dictionary<IBeltExtractor, long> _extractors = new();

        public float Speed { get; }
        public Memory<Direction> Directions { get; } //placeholder, no idea how to represent it
        public Memory<Coord> Tiles { get; }


        long _lastUpdate = 0;
        long _distanceUntilEvent = 0;

        public float TimeUntilEvent => _distanceUntilEvent / Speed;

        public void InsertItem( long currentTime )
        {
            long delta = currentTime - _lastUpdate;
            _items.Add( delta );
            _lastUpdate = currentTime;
            foreach( long extractorDistance in _extractors.Values )
            {
                _distanceUntilEvent = Math.Min( _distanceUntilEvent, extractorDistance );
            }
        }

        public IDisposable? RegisterExtractor( long position, IBeltExtractor beltExtractor )
        {
            _extractors.Add( beltExtractor, position );
            if( _items.Count == 0 ) return null;
            long itemDistance = 0;
            long itemDistanceLast = 0;
            for( int i = _items.Count - 1; i >= 0; i-- )
            {
                if( itemDistance > position )
                {
                    long distance = position - itemDistanceLast;
                    _distanceUntilEvent = Math.Min( distance, _distanceUntilEvent );
                }

                itemDistanceLast = itemDistance;
                itemDistance += _items[i];
            }
            return new Disposable( this, beltExtractor );
        }

        struct Disposable : IDisposable
        {
            readonly Belt _belt;
            readonly IBeltExtractor _beltExtractor;

            public Disposable( Belt belt, IBeltExtractor beltExtractor )
            {
                _belt = belt;
                _beltExtractor = beltExtractor;
            }
            public void Dispose() => _belt._extractors.Remove( _beltExtractor );
        }
    }
}
