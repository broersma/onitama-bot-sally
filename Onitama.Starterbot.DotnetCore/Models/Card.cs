using System;
using System.Collections.Generic;
using System.Linq;
using OnitamaTestClient.Models.Enums;

namespace OnitamaTestClient.Models {
    public class Card {
        public CardType Type { get; set; }
        public Position[] Targets { get; set; }

        public Card() {
        }
        public Card(CardType cardType, params Position[] targets) {
            Type = cardType;
            Targets = targets;
        }
        
        public Card(Card card) {
            Type = card.Type;
            Targets = card.Targets;
        }

        public Card flip() {
            var card = new Card(this);
            card.Targets = this.Targets.Select(p => new Position(-p.X,-p.Y)).ToArray();
            return card;
        }
    }
}