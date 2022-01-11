using Kuinox.ConveyorBelts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kuinox.ConveyorBelts
{
    public class Belt
    {
        /// <summary>
        /// Consider this a linked list.
        /// The order is reversed: items at the beginning of the list are at the end of the belt (they were the first added)
        /// Each item point to 
        /// </summary>
        readonly List<(float Distance, Item Item)> _items = new();
        readonly SortedBeltExtratorList _extractors = new();

        public float Speed { get; }
        //public Memory<Direction> Directions { get; } //placeholder, no idea how to represent it
        //public Memory<Vect> Tiles { get; }


        long _lastUpdateTime = 0;
        float _distancePadding = 0;
        public long TimeUntilEvent { get; private set; }

        void OnNewEvent( float distanceUntilEvent )
        {
            TimeUntilEvent = Math.Min( TimeUntilEvent, (long)(distanceUntilEvent / Speed) );
        }

        void TimeUpdate( long newTime )
        {
            long delta = newTime - _lastUpdateTime;
            _lastUpdateTime = newTime;
            TimeUntilEvent -= delta;
            _distancePadding += delta * Speed;
            Debug.Assert( TimeUntilEvent > 0 );
        }

        public void InsertItemAt( float distanceFromBeginning, long currentTime, Item item )
        {
            TimeUpdate( currentTime );
            if( _extractors.Any )
            {
                OnNewEvent( _extractors.DistanceFromNext( distanceFromBeginning ) );
            }
            float sum = _distancePadding;
            if( distanceFromBeginning < sum )
            {
                _items.Add( (_distancePadding - distanceFromBeginning, item) );
                _distancePadding = distanceFromBeginning;
                return;
            }
            for( int i = _items.Count - 1; i >= 0; i-- )
            {
                float newDistance = sum + _items[i].Distance;
                if( newDistance > distanceFromBeginning )
                {
                    _items[i] = new( distanceFromBeginning - sum, _items[i].Item );
                    _items.Insert( i, (newDistance - distanceFromBeginning, item) );
                    return;
                }
                sum = newDistance;
            }
            if( _items.Count == 0 )
            {
                _items.Add( (0, item) );
                _distancePadding = distanceFromBeginning;
                return;
            }
            _items[0] = new( distanceFromBeginning - sum, _items[0].Item );
            _items.Insert( 0, (0, item) );
        }

        public void AdvanceToNextEvent()
        {
            TimeUpdate( TimeUntilEvent );
            float sum = _distancePadding;
            int i = _items.Count - 1;
            foreach( (float distance, IBeltExtractor extractor) in _extractors )
            {
                for( ; i >= 0; i-- )
                {
                    bool shouldExtract = sum - distance < float.Epsilon * 10000;
                    sum += _items[i].Distance;
                    if( shouldExtract )
                    {
                        extractor.InsertItem( _lastUpdateTime, _items[i].Item );
                        RemoveItem( i );
                        continue;
                    }
                }
            }
        }


        void RemoveItem( int index )
        {
            if( index == _items.Count - 1 )
            {
                _distancePadding += _items[index].Distance;
            }
            else
            {
                (float distance, Item item) = _items[index + 1];
                _items[index + 1] = new( distance + _items[index].Distance, item );
            }
            _items.RemoveAt( index );
        }

        public IDisposable? RegisterExtractor( long positionToExtract, IBeltExtractor beltExtractor )
        {
            _extractors.Add( (positionToExtract, beltExtractor) );
            if( _items.Count == 0 ) return null;
            float itemDistance = 0;
            float itemDistanceLast = 0;
            for( int i = _items.Count - 1; i >= 0; i-- )
            {
                if( itemDistance > positionToExtract )
                {
                    float distance = positionToExtract - itemDistanceLast;
                    OnNewEvent( distance );
                }

                itemDistanceLast = itemDistance;
                itemDistance += _items[i].Distance;
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
