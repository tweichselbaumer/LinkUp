using LinkUp.Node.Logic;
using LinkUp.Raw;
using System;
using System.Linq;
using System.Threading.Tasks;

#if NET45 || NETCOREAPP2_0

using System.Timers;

#endif

namespace LinkUp.Node
{
    public class LinkUpSubNode : IDisposable
    {
        private LinkUpConnector _Connector;
        private bool _IsInitialized;
        private LinkUpNode _Master;
        private string _Name;
        private ushort _NextIdentifier = 1;
        private int _LostPings = 0;
        private object _LockObject = new object();
#if NET45 || NETCOREAPP2_0
        private Timer _PingTimer;
#endif

        internal LinkUpSubNode(LinkUpConnector connector, LinkUpNode master)
        {
            _Master = master;
            _Connector = connector;
            _Connector.ReveivedPacket += _Connector_ReveivedPacket;
#if NET45 || NETCOREAPP2_0
            _PingTimer = new Timer(500);
            _PingTimer.Elapsed += _PingTimer_Elapsed;
            _PingTimer.Start();
#endif
        }

        internal void CallFunction(LinkUpFunctionLabel linkUpFunctionLabel, byte[] data)
        {
            LinkUpFunctionCallRequest functionCallRequest = new LinkUpFunctionCallRequest();
            functionCallRequest.Identifier = linkUpFunctionLabel.ChildIdentifier;
            functionCallRequest.Data = data;
            _Connector?.SendPacket(functionCallRequest.ToPacket());
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

#if NET45 || NETCOREAPP2_0

        private void _PingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_IsInitialized)
            {
                _Connector.SendPacket(new LinkUpPingRequest().ToPacket());
                _LostPings++;
            }
            if (_LostPings > 20)
            {
                if (_IsInitialized)
                    //_Master.RemoveLabels(_Name);
                    _IsInitialized = false;
            }
        }

#endif

        private ushort GetNextIdentifier()
        {
            lock (_LockObject)
            {
                return _NextIdentifier++;
            }
        }

        internal LinkUpConnector Connector
        {
            get
            {
                return _Connector;
            }
        }

        public void Dispose()
        {
#if NET45 || NETCOREAPP2_0
            _PingTimer.Stop();
#endif
            Connector?.Dispose();
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
                                catch (Exception ex) { }
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
    }
}