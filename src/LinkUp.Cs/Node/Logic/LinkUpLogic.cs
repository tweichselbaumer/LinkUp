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

using LinkUp.Cs.Raw;

namespace LinkUp.Cs.Node.Logic
{
   internal abstract class LinkUpLogic
   {
      protected abstract void ParseFromRaw(byte[] data);

      protected abstract byte[] ToRaw();

      internal static LinkUpLogic ParseFromPacket(LinkUpPacket packet)
      {
         //TODO: implement checks
         LinkUpLogicType type = (LinkUpLogicType)packet.Data[0];
         LinkUpLogic logic = null;

         switch (type)
         {
            case LinkUpLogicType.NameRequest:
               logic = new LinkUpNameRequest();
               break;

            case LinkUpLogicType.NameResponse:
               logic = new LinkUpNameResponse();
               break;

            case LinkUpLogicType.PropertyGetRequest:
               logic = new LinkUpPropertyGetRequest();
               break;

            case LinkUpLogicType.PropertyGetResponse:
               logic = new LinkUpPropertyGetResponse();
               break;

            case LinkUpLogicType.PropertySetRequest:
               logic = new LinkUpPropertySetRequest();
               break;

            case LinkUpLogicType.PropertySetResponse:
               logic = new LinkUpPropertySetResponse();
               break;

            case LinkUpLogicType.PingRequest:
               logic = new LinkUpPingRequest();
               break;

            case LinkUpLogicType.PingResponse:
               logic = new LinkUpPingResponse();
               break;

            case LinkUpLogicType.EventFireRequest:
               logic = new LinkUpEventFireRequest();
               break;

            case LinkUpLogicType.EventFireResponse:
               logic = new LinkUpEventFireResponse();
               break;

            case LinkUpLogicType.EventSubscribeRequest:
               logic = new LinkUpEventSubscribeRequest();
               break;

            case LinkUpLogicType.EventSubscribeResponse:
               logic = new LinkUpEventSubscribeResponse();
               break;

            case LinkUpLogicType.EventUnsubscribeRequest:
               logic = new LinkUpEventUnsubscribeRequest();
               break;

            case LinkUpLogicType.EventUnsubscribeResponse:
               logic = new LinkUpEventUnsubscribeResponse();
               break;

            case LinkUpLogicType.FunctionCallRequest:
               logic = new LinkUpFunctionCallRequest();
               break;

            case LinkUpLogicType.FunctionCallResponse:
               logic = new LinkUpFunctionCallResponse();
               break;
         }

         logic?.ParseFromRaw(packet.Data);

         return logic;
      }

      internal LinkUpPacket ToPacket()
      {
         return new LinkUpPacket() { Data = ToRaw() };
      }
   }
}