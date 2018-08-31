using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OnitamaTestClient.Models;
using OnitamaTestClient.Models.Enums;
using OnitamaTestClient.Services;
using RemoteBotClient;

namespace OnitamaTestClient {
    abstract class TestBot {
        protected GameState gameState;
        private PlayerIdentityEnum identity;
        private readonly IBotInterface botInterface;

        public TestBot(IBotInterface botInterface) {
            this.botInterface = botInterface;
        }

        public void Run() {
            this.ReadGameInfo();

            while (true) {
                this.ReadGameState();
                var nextMove = this.GetNextMove();
                var asMessage = this.CreateMessage(nextMove);
                this.botInterface.WriteLine(Serialize(asMessage));
            }
        }
        protected void Log(string message) {
            this.botInterface.Log(message);
        }

        private void ReadGameInfo() {   
            var data = this.botInterface.ReadLine();

            var message = Deserialize<Message>(data);
            if (message.Type == MessageType.GameInfo) {
                var gameInfo = Deserialize<GameInfo>(message.JsonPayload);
                this.identity = gameInfo.Identity;
            }
            else {
                throw new Exception($"Expected to read GameInfo message, instead received: {message.Type}");
            }
        }

        private void ReadGameState() {
            var data = this.botInterface.ReadLine();

            var message = JsonConvert.DeserializeObject<Message>(data);
            if (message.Type == MessageType.NewGameState) {
                var gs = Deserialize<GameState>(message.JsonPayload);
                this.gameState = gs;
            }
            else
            {
                throw new Exception($"Expected to read NewGameState message, instead received: {message.Type}");
            }
        }
        protected abstract Move GetNextMove();
        private Message CreateMessage(Move command) {
            var message = new Message();
            if (command is Move.Play) {
                message.Type = MessageType.MovePiece;
            }

            if (command is Move.Pass) {
                message.Type = MessageType.Pass;
            }

            message.JsonPayload = Serialize(command);
            return message;
        }

        private static T Deserialize<T>(string serialized) where T : new()
        {
            return JsonConvert.DeserializeObject<T>(serialized, new StringEnumConverter());
        }

        private static string Serialize(object toSeriliaze)
        {
            return JsonConvert.SerializeObject(toSeriliaze);
        }
    }
}
