using OnitamaTestClient.Models.Enums;

namespace OnitamaTestClient.Models {
    public class Piece {
        public PlayerIdentityEnum Owner { get; set; }
        public PieceTypeEnum Type { get; set; }
        public Position PositionOnBoard { get; set; }

        public Piece() {
        }
        
        public Piece(Piece piece) {
            Owner = piece.Owner;
            Type = piece.Type;
            PositionOnBoard = piece.PositionOnBoard;
        }
        public Piece moveTo(Position position) {
            var piece = new Piece(this);
            piece.PositionOnBoard = position;
            return piece;
        }
    }
}
