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

namespace LinkUp.Cs.Datagram
{
   public abstract class DatagramProtocolDecorator : IDatagramProtocol
   {
      public DatagramProtocolDecorator(IDatagramProtocol nextLayer)
      {
         NextLayer = nextLayer;
         NextLayer.ReveivedDatagram += NextLayer_ReveivedDatagram;
      }

      public event ReveicedDatagramEventHandler ReveivedDatagram;

      protected IDatagramProtocol NextLayer { get; private set; }

      private void NextLayer_ReveivedDatagram(IDatagramProtocol sender, Datagram datagram)
      {
         ProgressReceived(datagram);
      }

      protected void OnReceived(Datagram datagram)
      {
         ReveivedDatagram?.Invoke(this, datagram);
      }

      public abstract void ProgressReceived(Datagram datagram);

      public abstract bool Send(Datagram datagram);
   }
}