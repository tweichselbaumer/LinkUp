/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2023 Thomas Weichselbaumer
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 ********************************************************************************/

namespace LinkUp.Cs.Raw
{
   internal class LinkUpBytesPerSecondCounter : IDisposable
   {
      private Queue<long> _Queue = new Queue<long>();
      private System.Timers.Timer _Timer;
      private long current;

      internal LinkUpBytesPerSecondCounter()
      {
         for (int i = 0; i < 5; i++)
         {
            _Queue.Enqueue(0);
         }

         _Timer = new System.Timers.Timer(1000);
         _Timer.Elapsed += _Timer_Elapsed;
         _Timer.Start();
      }

      internal double BytesPerSecond
      {
         get
         {
            double result;
            lock (_Queue)
            {
               result = _Queue.Sum() / _Queue.Count;
            }
            return result;
         }
      }

      public void Dispose()
      {
         if (_Timer != null)
         {
            _Timer.Dispose();
         }
      }

      internal void AddBytes(long bytes)
      {
         lock (_Queue)
         {
            current += bytes;
         }
      }

      private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
      {
         lock (_Queue)
         {
            _Queue.Enqueue(current);
            current = 0;
            _Queue.Dequeue();
         }
      }
   }
}