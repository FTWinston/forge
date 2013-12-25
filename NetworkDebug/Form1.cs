﻿using Neon.Network;
using Neon.Network.Lobby;
using Neon.Network.Chat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading.Tasks;
using Neon.Utilities;
using log4net;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using log4net.Core;
using log4net.Appender;
using Newtonsoft.Json;
using Neon.Network.Core;

namespace NetworkDebug {
    public partial class Form1 : Form, IAppender {
        public Form1() {
            InitializeComponent();

            TextLocalIP.Text = IPAddress.Loopback.ToString();

            Hierarchy hiearchy = (Hierarchy)LogManager.GetRepository();
            hiearchy.Root.Level = Level.All;
            hiearchy.Root.AddAppender(this);
            hiearchy.Configured = true;
        }

        private NetworkPlayer CreatePlayer() {
            return new NetworkPlayer() {
                Guid = Guid.NewGuid(),
                Name = TextName.Text
            };
        }

        private void ButtonConnect_Click(object sender, EventArgs e) {
            NetworkPlayer player = CreatePlayer();

            MapManager mapManager = new MapManager();
            string password = TextPassword.Text;

            string ip = TextIPToConnectTo.Text;
            Task.Factory.StartNew(() => {

                Maybe<LobbyMember> maybe = LobbyMember.JoinLobby(ip, player, mapManager, password).Result;
                if (maybe.IsEmpty) {
                    Invoke(new Action(() => {
                        TextConnectionInformation.Text = "Failed to connected";
                    }));
                }
                else {
                    LobbyMember member = maybe.Value;
                    _context = member.Context;
                    while (true) {
                        member.Update();
                        Invoke(new Action(() => {
                            string players = string.Join(", ", member.GetLobbyMembers().Select(p => p.Name).ToArray());
                            TextConnectionInformation.Text = "Lobby Players=" + players;
                        }));
                    }
                }

            });

        }

        private void ButtonStartServer_Click(object sender, EventArgs e) {
            NetworkPlayer player = CreatePlayer();

            LobbyHost.LobbySettings settings = new LobbyHost.LobbySettings() {
                MapManager = new MapManager(),
                Password = TextPassword.Text,
                SerializedMap = "",
            };

            LobbyHost host = LobbyHost.CreateLobby(player, settings);
            _context = host.Context;
            TextConnectionInformation.Text = "Created lobby";
            Task.Factory.StartNew(() => {
                while (true) {
                    host.Update();
                    Invoke(new Action(() => {
                        string players = string.Join(", ", host.GetLobbyMembers().Select(p => p.Name).ToArray());
                        TextConnectionInformation.Text = "Lobby Players=" + players;
                    }));
                }
            });
        }

        private NetworkContext _context;

        public void DoAppend(LoggingEvent loggingEvent) {
            string text = loggingEvent.Level.Name + " - " + loggingEvent.MessageObject.ToString() + "\n";

            Invoke(new Action(() => {
                LabelLog.Text += text;
            }));
        }

        private void ButtonSendAll_Click(object sender, EventArgs e) {
            _context.SendMessage(NetworkMessageRecipient.All, new DummyMessage());
        }

        private void ButtonSendClients_Click(object sender, EventArgs e) {
            _context.SendMessage(NetworkMessageRecipient.Clients, new DummyMessage());
        }

        private void ButtonSendServer_Click(object sender, EventArgs e) {
            _context.SendMessage(NetworkMessageRecipient.Server, new DummyMessage());
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class DummyMessage : INetworkMessage {

    }

    internal class MapManager : IMapManager {
        public Task<bool> HasMap(string mapHash) {
            return Task.Factory.StartNew(() => true);
        }

        public string GetHash(string serializedMap) {
            return "";
        }

        public void Save(string serializedMap) {
        }
    }
}