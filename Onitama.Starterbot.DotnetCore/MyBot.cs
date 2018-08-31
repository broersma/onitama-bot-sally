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
    class MyBot : TestBot {
        public MyBot(IBotInterface botInterface) : base(botInterface) {
            
        }
        protected override Move GetNextMove() {
            var node = new Node(this.gameState, this.gameState.CurrentlyPlaying);
            node.Minimax(4);
            if ( node.BestMove != null ) {
                this.Log("Playing " + node.BestMove.UsedCard + " " + node.BestMove.From + " -> " + node.BestMove.To);
                return node.BestMove;
            } else {
                return new Move.Pass(this.gameState.MyHand.First().Type);
            }
        }
    }
}