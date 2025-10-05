using System;
using System.Collections.Generic;

namespace StrategyGame.Utils {
    public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority> {
        private List<(TElement, TPriority)> _heap;
        public int Count => _heap.Count;
        public PriorityQueue() {
            _heap = new List<(TElement, TPriority)>();
        }
        public void Enqueue(TElement element, TPriority priority) {
            _heap.Add((element, priority));
            HeapifyUp(_heap.Count - 1);
        }
        public TElement Dequeue() {
            if (_heap.Count == 0) {
                throw new InvalidOperationException("Priority queue is empty.");
            }
            var highestPriorityElement = _heap[0];
            _heap[0] = _heap[^1];
            _heap.RemoveAt(_heap.Count - 1);
            if (_heap.Count > 0) {
                HeapifyDown(0);
            }
            return highestPriorityElement.Item1;
        }
        private void HeapifyUp(int index) {
            var parentIndex = (index - 1) / 2;
            while (index > 0 && _heap[index].Item2.CompareTo(_heap[parentIndex].Item2) < 0) {
                (_heap[index], _heap[parentIndex]) = (_heap[parentIndex], _heap[index]);
                index = parentIndex;
                parentIndex = (index - 1) / 2;
            }
        }
        private void HeapifyDown(int index) {
            int leftChildIndex;
            int rightChildIndex;
            int smallestChildIndex = index;
            while (true) {
                leftChildIndex = 2 * index + 1;
                rightChildIndex = 2 * index + 2;
                if (leftChildIndex < _heap.Count && _heap[leftChildIndex].Item2.CompareTo(_heap[smallestChildIndex].Item2) < 0) {
                    smallestChildIndex = leftChildIndex;
                }
                if (rightChildIndex < _heap.Count && _heap[rightChildIndex].Item2.CompareTo(_heap[smallestChildIndex].Item2) < 0) {
                    smallestChildIndex = rightChildIndex;
                }
                if (smallestChildIndex == index) {
                    break;
                }
                (_heap[index], _heap[smallestChildIndex]) = (_heap[smallestChildIndex], _heap[index]);
                index = smallestChildIndex;
            }
        }
        private TElement Peek() {
            if (_heap.Count == 0) {
                throw new InvalidOperationException("Priority queue is empty.");
            }
            return _heap[0].Item1;
        }
    }
}
