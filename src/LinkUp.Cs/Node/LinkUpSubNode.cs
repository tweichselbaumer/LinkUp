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

using LinkUp.Cs.Node.Logic;
using LinkUp.Cs.Raw;

namespace LinkUp.Cs.Node
{
   public class LinkUpSubNode : IDisposable
   {
      private LinkUpConnector _Connector;
      private bool _IsInitialized;
      private object _LockObject = new object();
      private int _LostPings = 0;
      private LinkUpNode _Master;
      private string _Name;
      private ushort _NextIdentifier = 1;
      private System.Timers.Timer _PingTimer;

      internal LinkUpSubNode(LinkUpConnector connector, LinkUpNode master)
      {
         _Master = master;
         _Connector = connector;
         _Connector.ReveivedPacket += _Connector_ReveivedPacket;

         _PingTimer = new System.Timers.Timer(500);
         _PingTimer.Elapsed += _PingTimer_Elapsed;
         _PingTimer.Start();
      }

      public LinkUpConnector Connector
      {
         get
         {
            return _Connector;
         }
      }

      public void Dispose()
      {
         _PingTimer.Stop();

         Connector?.Dispose();
      }

      internal void CallFunction(LinkUpFunctionLabel linkUpFunctionLabel, byte[] data)
      {
         LinkUpFunctionCallRequest functionCallRequest = new LinkUpFunctionCallRequest();
         functionCallRequest.Identifier = linkUpFunctionLabel.ChildIdentifier;
         functionCallRequest.Data = data;
         _Connector?.SendPacket(functionCallRequest.ToPacket());
      }

      internal void GetProperty(LinkUpPropertyLabelBase linkUpPrimitiveBaseLabel)
      {
         LinkUpPropertyGetRequest propertyGetRequest = new LinkUpPropertyGetRequest();
         propertyGetRequest.Identifier = linkUpPrimitiveBaseLabel.ChildIdentifier;
         _Connector?.SendPacket(propertyGetRequest.ToPacket());
      }

      internal void SetProperty(LinkUpPropertyLabelBase linkUpPrimitiveBaseLabel, byte[] data)
      {
         LinkUpPropertySetRequest propertySetRequest = new LinkUpPropertySetRequest();
         propertySetRequest.Identifier = linkUpPrimitiveBaseLabel.ChildIdentifier;
         propertySetRequest.Data = data;
         _Connector?.SendPacket(propertySetRequest.ToPacket());
      }

      internal void SubscribeEvent(LinkUpEventLabel linkUpEventLabel)
      {
         LinkUpEventSubscribeRequest eventSubscribeRequest = new LinkUpEventSubscribeRequest();
         eventSubscribeRequest.Identifier = linkUpEventLabel.ChildIdentifier;
         _Connector?.SendPacket(eventSubscribeRequest.ToPacket());
      }

      internal void UnsubscribeEvent(LinkUpEventLabel linkUpEventLabel)
      {
         LinkUpEventUnsubscribeRequest eventUnsubscribeRequest = new LinkUpEventUnsubscribeRequest();
         eventUnsubscribeRequest.Identifier = linkUpEventLabel.ChildIdentifier;
         _Connector?.SendPacket(eventUnsubscribeRequest.ToPacket());
      }

      private void _Connector_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
      {
         try
         {
            LinkUpLogic logic = LinkUpLogic.ParseFromPacket(packet);
            if (logic is LinkUpNameRequest)
            {
               LinkUpNameRequest nameRequest = logic as LinkUpNameRequest;

               if (nameRequest.LabelType == LinkUpLabelType.Node)
               {
                  _IsInitialized = true;
                  _Name = nameRequest.Name;

                  LinkUpNameResponse nameResponse = new LinkUpNameResponse();
                  nameResponse.Name = nameRequest.Name;
                  nameResponse.Identifier = 0;
                  nameResponse.LabelType = LinkUpLabelType.Node;
                  //_Master.RemoveLabels(_Name);
                  Task.Run(() =>
                  {
                     _Connector.SendPacket(nameResponse.ToPacket());
                  });
               }
               if (nameRequest.LabelType != LinkUpLabelType.Node)
               {
                  LinkUpLabel label = _Master.AddSubLabel(string.Format("{0}/{1}", _Name, nameRequest.Name), nameRequest.LabelType, nameRequest.Options);
                  if (label.ChildIdentifier == 0)
                  {
                     label.ChildIdentifier = GetNextIdentifier();
                     //Console.WriteLine("ID: {0} Name: {1}", label.ChildIdentifier, label.Name);
                  }
                  label.Owner = this;

                  LinkUpNameResponse nameResponse = new LinkUpNameResponse();
                  nameResponse.Name = nameRequest.Name;
                  nameResponse.Identifier = label.ChildIdentifier;
                  nameResponse.LabelType = nameRequest.LabelType;

                  _Connector.SendPacket(nameResponse.ToPacket());

                  if (label.LabelType == LinkUpLabelType.Event)
                  {
                     Task.Run(() =>
                     {
                        try
                        {
                           (label as LinkUpEventLabel).Resubscribe();
                        }
                        catch (Exception) { }
                     });
                  }
               }
            }
            else if (logic is LinkUpEventFireRequest)
            {
               LinkUpEventFireRequest eventFireRequest = (LinkUpEventFireRequest)logic;
               LinkUpLabel label = _Master.Labels.FirstOrDefault(c => c.ChildIdentifier == eventFireRequest.Identifier);
               if (label != null)
               {
                  if (label is LinkUpEventLabel)
                  {
                     (label as LinkUpEventLabel).DoEvent(eventFireRequest.Data);
                  }
               }
            }
            else if (logic is LinkUpEventSubscribeResponse)
            {
               LinkUpEventSubscribeResponse eventSubscribeResponse = (LinkUpEventSubscribeResponse)logic;
               LinkUpLabel label = _Master.Labels.FirstOrDefault(c => c.ChildIdentifier == eventSubscribeResponse.Identifier);
               if (label != null)
               {
                  if (label is LinkUpEventLabel)
                  {
                     (label as LinkUpEventLabel).SubscribeDone();
                  }
               }
            }
            else if (logic is LinkUpEventUnsubscribeResponse)
            {
               LinkUpEventUnsubscribeResponse eventUnsubscribeResponse = (LinkUpEventUnsubscribeResponse)logic;
               LinkUpLabel label = _Master.Labels.FirstOrDefault(c => c.ChildIdentifier == eventUnsubscribeResponse.Identifier);
               if (label != null)
               {
                  if (label is LinkUpEventLabel)
                  {
                     (label as LinkUpEventLabel).UnsubscribeDone();
                  }
               }
            }
            else if (logic is LinkUpPropertyGetResponse)
            {
               LinkUpPropertyGetResponse propertyGetResponse = (LinkUpPropertyGetResponse)logic;
               LinkUpLabel label = _Master.Labels.FirstOrDefault(c => c.ChildIdentifier == propertyGetResponse.Identifier);
               if (label != null)
               {
                  if (label is LinkUpPropertyLabelBase)
                  {
                     (label as LinkUpPropertyLabelBase).GetDone(propertyGetResponse.Data);
                  }
               }
            }
            else if (logic is LinkUpPropertySetResponse)
            {
               LinkUpPropertySetResponse propertySetResponse = (LinkUpPropertySetResponse)logic;
               LinkUpLabel label = _Master.Labels.FirstOrDefault(c => c.ChildIdentifier == propertySetResponse.Identifier);
               if (label != null)
               {
                  if (label is LinkUpPropertyLabelBase)
                  {
                     (label as LinkUpPropertyLabelBase).SetDone();
                  }
               }
            }
            else if (logic is LinkUpFunctionCallResponse)
            {
               LinkUpFunctionCallResponse functionCallResponse = (LinkUpFunctionCallResponse)logic;
               LinkUpLabel label = _Master.Labels.FirstOrDefault(c => c.ChildIdentifier == functionCallResponse.Identifier);
               if (label != null)
               {
                  if (label is LinkUpFunctionLabel)
                  {
                     (label as LinkUpFunctionLabel).DoEvent(functionCallResponse.Data);
                  }
               }
            }
            else if (logic is LinkUpPingResponse)
            {
               _LostPings = 0;
            }
         }
         catch (Exception)
         {
         }
      }

      private void _PingTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
      {
         if (_Connector.ConnectivityState == LinkUpConnectivityState.Connected)
            _Connector.SendPacket(new LinkUpPingRequest().ToPacket());
         if (_IsInitialized)
         {
            _LostPings++;
         }
         if (_LostPings > 20)
         {
            if (_IsInitialized)
               //_Master.RemoveLabels(_Name);
               _IsInitialized = false;
         }
      }

      private ushort GetNextIdentifier()
      {
         lock (_LockObject)
         {
            return _NextIdentifier++;
         }
      }
   }
}