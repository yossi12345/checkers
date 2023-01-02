using System.Collections.Generic;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;

namespace chess_rewrite2
{
    internal class ChessGame
    {

        int amountOfMovesInGameWithoutPawnMoveOrCapturing = 100;
        Piece[,] currentPosition;
        Piece[,,] positionsHistory;
        int moveCountWithoutPawnMoveOrCapturing;
        bool isWhiteTurn;
        static void Main(string[] args)
        {
            new ChessGame().playChess();
        }
        void playChess()
        {
            Piece[,] copy;
            int pawnStartRow,pawnEndRow,endColumnEnPassantRight=-1, endColumnEnPassantLeft=-1, endRowEnPassantRight=-1, endRowEnPassantLeft=-1;
            int beginColumnEnPassantRight = -1;
            int beginColumnEnPassantLeft = -1;
            int beginRowEnPassantRight = -1;
            int beginRowEnPassantLeft = -1;
            isWhiteTurn = true;
            bool pieceEaten = false, moveMade = true, isEnPassantPossible = false, theUserWillMakeEnPassantMove;
            int countFullFloorsInPositionHistory = 1;
            inializeDefaultStartingPosition();
            initializePositionHistory();
            setPositionInPositionsHistory(0);
            string input,complain="";
            int beginColumnSquare, endColumnSquare, beginRowSquare, endRowSquare;
            int endRowEnPassant, endColumnEnPassant;
            moveCountWithoutPawnMoveOrCapturing = 0;
            while (true) 
            {
                printPosition();
                Console.WriteLine(countFullFloorsInPositionHistory);
                if (isGameOver(isEnPassantPossible, countFullFloorsInPositionHistory))
                    return;
                if (isCheck())
                    Console.WriteLine("CHECK");
                Console.Write(complain);
                input = getInput();
                if (input == "draw")
                {
                    Console.WriteLine("ok it is a draw");
                    return;
                }
                beginColumnSquare = (int)input[0] - 97;
                endColumnSquare = (int)input[2] - 97;
                beginRowSquare = (int)input[1] - 49;
                endRowSquare = (int)input[3] - 49;
                if (currentPosition[beginRowSquare, beginColumnSquare] == null)
                    complain = "there is no piece in the beginning square. ";
                else if (currentPosition[beginRowSquare, beginColumnSquare].getIsWhite() != isWhiteTurn)
                    complain = "the piece in this square isn't yours. ";
                else
                {
                    pieceEaten = false;
                    theUserWillMakeEnPassantMove = false;
                    if (currentPosition[endRowSquare, endColumnSquare] != null)
                        pieceEaten = true;
                    if ((currentPosition[beginRowSquare, beginColumnSquare] is Pawn) && currentPosition[endRowSquare, endColumnSquare] == null && endColumnSquare != beginColumnSquare)
                        theUserWillMakeEnPassantMove = true;

                    if (makeMoveIfLegal(beginRowSquare, beginColumnSquare, endRowSquare, endColumnSquare,isEnPassantPossible))
                    {
                        if (isEnPassantPossible&&!theUserWillMakeEnPassantMove)
                        {
                            copy = getPositionCopy();
                            isWhiteTurn = isWhiteTurn ? false : true;
                            if (beginColumnEnPassantRight != -1)
                            {
                                makeEnPassant(beginRowEnPassantRight, beginColumnEnPassantRight, endRowEnPassantRight, endColumnEnPassantRight);
                                if (!isCheck())
                                {
                                    initializePositionHistory();
                                    countFullFloorsInPositionHistory = 0;
                                }
                            }
                            if (beginColumnEnPassantLeft != -1)
                            {
                                makeEnPassant(beginRowEnPassantLeft, beginColumnEnPassantLeft, endRowEnPassantLeft, endColumnEnPassantLeft);
                                if (!isCheck())
                                {
                                    initializePositionHistory();
                                    countFullFloorsInPositionHistory = 0;
                                }
                            }
                            currentPosition = copy;
                            isWhiteTurn = isWhiteTurn ? false : true;
                        }
                        isEnPassantPossible = false;
                        endColumnEnPassantRight = -1;
                        endColumnEnPassantLeft = -1;
                        endRowEnPassantRight = -1;
                        endRowEnPassantLeft = -1;
                        beginColumnEnPassantRight = -1;
                        beginColumnEnPassantLeft = -1;
                        beginRowEnPassantRight = -1;
                        beginRowEnPassantLeft = -1;
                        if (currentPosition[endRowSquare, endColumnSquare] is Pawn)
                        {
                            if (endRowSquare == 0 || endRowSquare == 7)
                                pawnGetToEndLine(endRowSquare, endColumnSquare);
                            moveCountWithoutPawnMoveOrCapturing = -1;
                            initializePositionHistory();
                            countFullFloorsInPositionHistory = 0;
                            pawnStartRow = isWhiteTurn ? 1 : 6;
                            pawnEndRow = isWhiteTurn ? 3 : 4;
                            if (pawnStartRow == beginRowSquare && pawnEndRow == endRowSquare)
                            {
                                if (endColumnSquare + 1 < 8 && (currentPosition[pawnEndRow, endColumnSquare + 1] is Pawn) && isWhiteTurn != currentPosition[pawnEndRow, endColumnSquare + 1].getIsWhite())
                                {
                                    isEnPassantPossible = true;
                                    beginColumnEnPassantRight = endColumnSquare + 1;
                                    beginRowEnPassantRight = pawnEndRow;
                                    endRowEnPassantRight = pawnEndRow == 3 ? 2 : 5;
                                    endColumnEnPassantRight = beginColumnSquare;
                                }
                                if (endColumnSquare - 1 >= 0 && (currentPosition[pawnEndRow, endColumnSquare - 1] is Pawn) && isWhiteTurn != currentPosition[pawnEndRow, endColumnSquare - 1].getIsWhite())
                                {
                                    isEnPassantPossible = true;
                                    beginColumnEnPassantLeft = endColumnSquare - 1;
                                    beginRowEnPassantLeft = pawnEndRow;
                                    endRowEnPassantLeft = pawnEndRow == 3 ? 2 : 5;
                                    endColumnEnPassantLeft = beginColumnSquare;
                                }
                            }
                        }
                        else if (currentPosition[endRowSquare, endColumnSquare] is King)
                        {
                            if (!((King)currentPosition[endRowSquare, endColumnSquare]).getAlreadyMove())
                            {
                                if ((currentPosition[beginRowSquare, 0] is Rook) && !((Rook)currentPosition[beginRowSquare, 0]).getAlreadyMove())
                                {
                                    initializePositionHistory();
                                    countFullFloorsInPositionHistory = 0;
                                }
                                if ((currentPosition[beginRowSquare, 7] is Rook) && !((Rook)currentPosition[beginRowSquare, 7]).getAlreadyMove())
                                {
                                    initializePositionHistory();
                                    countFullFloorsInPositionHistory = 0;
                                }
                            }
                            ((King)currentPosition[endRowSquare, endColumnSquare]).setAlreadyMove(true);
                        }
                        else if (currentPosition[endRowSquare, endColumnSquare] is Rook)
                        {
                            if (!((Rook)currentPosition[endRowSquare, endColumnSquare]).getAlreadyMove())
                            {
                                if ((currentPosition[beginRowSquare, 4] is King) && !((King)currentPosition[beginRowSquare, 4]).getAlreadyMove())
                                {
                                    initializePositionHistory();
                                    countFullFloorsInPositionHistory = 0;
                                }
                            }
                            ((Rook)currentPosition[endRowSquare, endColumnSquare]).setAlreadyMove(true);
                        }
                        else if (pieceEaten)
                        {
                            moveCountWithoutPawnMoveOrCapturing = -1;
                            initializePositionHistory();
                            countFullFloorsInPositionHistory = 0;
                        }
                        setPositionInPositionsHistory(countFullFloorsInPositionHistory);
                        countFullFloorsInPositionHistory++;
                        moveCountWithoutPawnMoveOrCapturing++;
                        complain = "";
                        isWhiteTurn = isWhiteTurn ? false : true;
                    }
                    else
                        complain = "this move is illegal. ";
                }
            } 
        }

        void initializePositionHistory()
        {
            positionsHistory = new Piece[currentPosition.GetLength(0), currentPosition.GetLength(1), amountOfMovesInGameWithoutPawnMoveOrCapturing + 1];
            //Console.WriteLine("INItialize");
        }
        void inializeDefaultStartingPosition()
        {
            currentPosition = new Piece[8, 8];
            for (int i = 0; i < currentPosition.GetLength(1); i++)
            {
                currentPosition[1, i] = new Pawn(true);
                currentPosition[6, i] = new Pawn(false);
            }
            currentPosition[0, 0] = new Rook(true);
            currentPosition[0, 7] = new Rook(true);
            currentPosition[7, 0] = new Rook(false);
            currentPosition[7, 7] = new Rook(false);

            currentPosition[0, 1] = new Knight(true);
            currentPosition[0, 6] = new Knight(true);
            currentPosition[7, 1] = new Knight(false);
            currentPosition[7, 6] = new Knight(false);

            currentPosition[0, 2] = new Bishop(true);
            currentPosition[0, 5] = new Bishop(true);
            currentPosition[7, 2] = new Bishop(false);
            currentPosition[7, 5] = new Bishop(false);

            currentPosition[0, 3] = new Queen(true);
            currentPosition[0, 4] = new King(true);
            currentPosition[7, 3] = new Queen(false);
            currentPosition[7, 4] = new King(false);
        }//the board begin from leftdown white rook knight bishop queen king bishop knight rook
        void setmoveCountWithoutPawnMoveOrCapturing(int value)
        {
            if (value > 0)
                moveCountWithoutPawnMoveOrCapturing = value;
        }
        int getmoveCountWithoutPawnMoveOrCapturing() { return moveCountWithoutPawnMoveOrCapturing; }
        void setPositionInPositionsHistory(int floor)
        {
            for (int i = 0; i < currentPosition.GetLength(0); i++)
            {
                for (int j = 0; j < currentPosition.GetLength(1); j++)
                    positionsHistory[i, j, floor] = currentPosition[i, j];
            }
        }
        string getInput()
        {
            string input, complain = "";
            while (true)
            {
                if (complain != "")
                    printPosition();
                Console.WriteLine(complain + (isWhiteTurn ? "WHITE" : "BLACK") + " please enter your move or draw if you want to stop");
                input = Console.ReadLine();
                input = input.Trim();
                input = input.ToLower();
                complain = "the move wasn't entered correctly. ";
                if (input == "draw")
                    return input;
                if (input.Length == 4)
                    if (((int)input[0]) >= 97 && ((int)input[0]) <= 104)
                        if (((int)input[2]) >= 97 && ((int)input[2]) <= 104)
                            if (((int)input[1]) >= 49 && ((int)input[1]) <= 56)
                                if (((int)input[3]) >= 49 && ((int)input[3]) <= 56)
                                    return input;
            }
        }
        void printPosition()
        {
            Console.Write("  ");
            for (int i = 65; i <= 72; i++)
                Console.Write(" " + (char)i + " ");

            Console.WriteLine();
            for (int i = currentPosition.GetLength(0) - 1; i >= 0; i--)
            {
                Console.Write((i + 1) + "  ");
                for (int j = 0; j < currentPosition.GetLength(1); j++)
                {
                    Console.Write((currentPosition[i, j] == null ? "--" : currentPosition[i, j]) + " ");
                }
                Console.WriteLine();
            }
        }
        bool isGameOver(bool isEnPassantPossible,int countFullFloorsInPositionHistory)
        {
            if (isDeadPosition())
            {
                Console.WriteLine("it is dead position");
                return true;
            }
            if (!isLegalMoveExist(isEnPassantPossible))
            {
                if (isCheck())
                    Console.WriteLine("CHECKMATE the " + (isWhiteTurn ? "black" : "white") + " won");
                else
                    Console.WriteLine("STALEMATE");
                return true;
            }
            if (moveCountWithoutPawnMoveOrCapturing== amountOfMovesInGameWithoutPawnMoveOrCapturing)
            {
                Console.WriteLine("draw because it is the " + amountOfMovesInGameWithoutPawnMoveOrCapturing/2+"th move without capturing a piece or moving a pawn");
                return true;
            }
            if (is3PositionsInPositionHistoryAreTheSame(countFullFloorsInPositionHistory))
            {
                Console.WriteLine("draw because it is the third time that this position happens");
                return true;
            }
            return false;

        }
        bool is3PositionsInPositionHistoryAreTheSame(int countFullFloorsInPositionHistory)
        {
            if (countFullFloorsInPositionHistory < 4)
                return false;
            bool _2IdenticalPositionsFound = false;
            int upperFloor = countFullFloorsInPositionHistory - 1;
            int lowerFloor = (countFullFloorsInPositionHistory - 1)%2==0?0:1;
            while (lowerFloor < upperFloor)
            {
                if (compare2PositionsInPositionHistory(lowerFloor, upperFloor))
                {
                    if (_2IdenticalPositionsFound)
                        return true;
                    _2IdenticalPositionsFound = true;
                }
                lowerFloor += 2;
            }
            return false; 
        }
        bool compare2PositionsInPositionHistory(int floor1,int floor2)
        {
            int countSameSquaresInDifferentPositions = 0;
            for (int j = 0; j < positionsHistory.GetLength(0); j++)
            {
                for (int k = 0; k < positionsHistory.GetLength(1); k++)
                {
                    if ((positionsHistory[j, k, floor1] == null && positionsHistory[j, k,floor2] == null) || (positionsHistory[j, k, floor1] != null&& positionsHistory[j, k,floor1].Equals(positionsHistory[j, k,floor2])))
                        countSameSquaresInDifferentPositions++;
                }
            }
            if (countSameSquaresInDifferentPositions == positionsHistory.GetLength(0) * positionsHistory.GetLength(1))
                return true;
            return false;
        } 
        bool isDeadPosition()
        {
            int amountOfPieces = 0;
            bool isBishopOrKnightExist = false;
            for (int i = 0; i < currentPosition.GetLength(0); i++)
            {
                for (int j = 0; j < currentPosition.GetLength(1); j++)
                {
                    if (currentPosition[i, j] != null)
                    {
                        amountOfPieces++;
                        if ((currentPosition[i, j] is Bishop) || (currentPosition[i, j] is Knight))
                            isBishopOrKnightExist = true;
                    }
                    if (amountOfPieces > 3)
                        return false;
                }
            }
            if (amountOfPieces == 2 || isBishopOrKnightExist)
                return true;
            //if there are 3 pieces: 2 kings and piece which isn't knight or bishop
            return false;
        }
        bool isLegalMoveExist(bool isEnPassantPossible)
        {
            Piece[,] copy=getPositionCopy();
            bool check;
            for (int beginRowSquare = 0; beginRowSquare < currentPosition.GetLength(0); beginRowSquare++)
            {
                for (int beginColumnSquare = 0; beginColumnSquare < currentPosition.GetLength(1); beginColumnSquare++)
                {
                    if (currentPosition[beginRowSquare, beginColumnSquare] != null && currentPosition[beginRowSquare, beginColumnSquare].getIsWhite() == isWhiteTurn)
                    {
                        for (int endRowSquare = 0; endRowSquare < currentPosition.GetLength(0); endRowSquare++)
                        {
                            for (int endColumnSquare = 0; endColumnSquare < currentPosition.GetLength(1); endColumnSquare++)
                            {
                                if (currentPosition[beginRowSquare, beginColumnSquare].isPotentialMove(currentPosition, beginRowSquare, beginColumnSquare, endRowSquare, endColumnSquare))
                                {
                                    if (makeMoveIfLegal(beginRowSquare, beginColumnSquare, endRowSquare, endColumnSquare, isEnPassantPossible))
                                    {
                                        currentPosition = copy;
                                        return true;
                                    } 
                                } 
                            }
                        }
                    }
                }
            }
            return false;
        }
        Piece[,] getPositionCopy()
        {
            Piece[,] copy = new Piece[currentPosition.GetLength(0), currentPosition.GetLength(1)];
            for (int i = 0; i < currentPosition.GetLength(0); i++)
            {
                for (int j = 0; j < currentPosition.GetLength(1); j++)
                    copy[i, j] = currentPosition[i, j];
            }
            return copy;
        }
        bool makeCastleIfLegal(int beginRowSquare, int beginColumnSquare, int endRowSquare, int endColumnSquare)
        {
            if (!(currentPosition[beginRowSquare, beginColumnSquare] is King))
                return false;
            if (beginRowSquare != endRowSquare || ((King)currentPosition[beginRowSquare, beginColumnSquare]).getAlreadyMove() || ((endColumnSquare - beginColumnSquare != 2) && (endColumnSquare - beginColumnSquare != -2)))
                return false;
            if (endColumnSquare - beginColumnSquare == 2)//small castle
            {
                if (!(currentPosition[endRowSquare, 7] is Rook) || ((Rook)currentPosition[endRowSquare, 7]).getAlreadyMove())
                    return false;
                if (currentPosition[endRowSquare, 6] != null || currentPosition[endRowSquare, 5] != null)
                    return false;
            }
            else if (endColumnSquare - beginColumnSquare == -2)//big castle
            {
                if (!(currentPosition[endRowSquare, 0] is Rook) || ((Rook)currentPosition[endRowSquare, 0]).getAlreadyMove())
                    return false;
                if (currentPosition[endRowSquare, 2] != null && currentPosition[endRowSquare, 1] != null || currentPosition[endRowSquare, 3] != null)
                    return false;
            }
            int direction = endColumnSquare - beginColumnSquare == 2 ? 1 : -1;
            Piece[,] copy = getPositionCopy();
            makeRegularMove(beginRowSquare, beginColumnSquare, beginRowSquare, beginColumnSquare+direction);
            if (isCheck())
            {
                currentPosition = copy;
                return false;
            }
            makeRegularMove(beginRowSquare, beginColumnSquare + direction,beginRowSquare, beginColumnSquare + 2*direction);
            if (isCheck())
            {
                currentPosition = copy;
                return false;
            }
            currentPosition = copy;
            makeCastle(beginRowSquare, beginColumnSquare, endRowSquare, endColumnSquare);
            return true;
        }
        void makeCastle(int beginRowSquare, int beginColumnSquare, int endRowSquare, int endColumnSquare)
        {
            currentPosition[endRowSquare, endColumnSquare] = currentPosition[beginRowSquare, beginColumnSquare];
            currentPosition[beginRowSquare, beginColumnSquare] = null;
            if (endColumnSquare == 2)
            {
                currentPosition[beginRowSquare, 3] = currentPosition[beginRowSquare, 0];
                currentPosition[beginRowSquare, 0] = null;
            }
            else //if endColumnSquare==6
            {
                currentPosition[beginRowSquare, 5] = currentPosition[beginRowSquare, 7];
                currentPosition[beginRowSquare, 7] = null;
            }
        }
        void makeEnPassant(int beginRowSquare, int beginColumnSquare, int endRowSquare, int endColumnSquare)
        {
            int upOrDown = currentPosition[beginRowSquare, beginColumnSquare].getIsWhite() ? 1 : -1;
            makeRegularMove(beginRowSquare,beginColumnSquare,endRowSquare, endColumnSquare);
            currentPosition[endRowSquare - upOrDown, endColumnSquare] = null;

        }
        bool isCheck()
        {
            bool isWhiteKing = isWhiteTurn;
            int rowKing=-50,columnKing=-50;
            bool kingPlaceFound = false;
            for (int row = 0; row < currentPosition.GetLength(0) && !kingPlaceFound; row++)
            {
                for (int column = 0; column < currentPosition.GetLength(1) && !kingPlaceFound; column++)
                {
                    if ((currentPosition[row, column] is King) && (currentPosition[row, column].getIsWhite() == isWhiteKing))
                    {
                        rowKing = row;
                        columnKing = column;
                        kingPlaceFound = true;
                    }
                }
            }
            for (int rowPiece = 0; rowPiece < currentPosition.GetLength(0); rowPiece++)
            {
                for (int columnPiece = 0; columnPiece < currentPosition.GetLength(1); columnPiece++)
                {
                    if (currentPosition[rowPiece, columnPiece]!=null&& currentPosition[rowPiece, columnPiece].getIsWhite() != isWhiteKing)
                    {
                        if (currentPosition[rowPiece, columnPiece].isPotentialMove(currentPosition, rowPiece, columnPiece,rowKing,columnKing))
                            return true;
                    }
                }
            }
            return false;
        }
        void makeRegularMove(int beginRowSquare, int beginColumnSquare, int endRowSquare, int endColumnSquare)
        {
            currentPosition[endRowSquare, endColumnSquare] = currentPosition[beginRowSquare, beginColumnSquare];
            currentPosition[beginRowSquare, beginColumnSquare] = null;  
        }    
        void pawnGetToEndLine(int rowSquare, int columnSquare)
        {
            string input;
            string complain = "";
            bool isWhitePawn = currentPosition[rowSquare, columnSquare].getIsWhite();
            do
            {
                if (complain != "")
                    printPosition();
                Console.WriteLine("which piece would you like to get?");
                Console.WriteLine("enter knight, rook, queen or bishop." + complain);
                input = Console.ReadLine();
                input = input.Trim();
                input = input.ToLower();
                complain = " Please enter one of these four words exatly";
            } while (input != "knight" && input != "rook" && input != "queen" && input != "bishop");
            switch (input)
            {
                case "knight":
                    currentPosition[rowSquare, columnSquare] = new Knight(isWhitePawn);
                    break;
                case "rook":
                    currentPosition[rowSquare, columnSquare] = new Rook(isWhitePawn);
                    break;
                case "queen":
                    currentPosition[rowSquare, columnSquare] = new Queen(isWhitePawn);
                    break;
                case "bishop":
                    currentPosition[rowSquare, columnSquare] = new Bishop(isWhitePawn);
                    break;
            }
        }
        bool makeMoveIfLegal(int beginRowSquare, int beginColumnSquare, int endRowSquare, int endColumnSquare,bool isEnPassantPossible)
        {
            Piece[,] copy=getPositionCopy();
            bool enPassantDone = false;
            if (currentPosition[beginRowSquare, beginColumnSquare] == null)
                return false;

            if ((currentPosition[beginRowSquare, beginColumnSquare] is King) && (beginColumnSquare - endColumnSquare == 2 || beginColumnSquare - endColumnSquare == -2))
                return makeCastleIfLegal(beginRowSquare, beginColumnSquare, endRowSquare, endColumnSquare);
            if ((currentPosition[beginRowSquare, beginColumnSquare] is Pawn) && currentPosition[endRowSquare, endColumnSquare] == null && endColumnSquare != beginColumnSquare)
            {
                if (isEnPassantPossible)
                {
                    makeEnPassant(beginRowSquare, beginColumnSquare, endRowSquare, endColumnSquare);
                    enPassantDone = true;
                }
                else
                    return false;
            }
            else if (!currentPosition[beginRowSquare, beginColumnSquare].isPotentialMove(currentPosition,beginRowSquare,beginColumnSquare,endRowSquare,endColumnSquare))
                return false;
            if (!enPassantDone)
                makeRegularMove(beginRowSquare, beginColumnSquare, endRowSquare, endColumnSquare);

            if (isCheck())
            {
                currentPosition = copy;
                return false;
            }
            return true;
        }
    }
    class Piece
    {
        string name;
        bool isWhite;
        public Piece(string name, bool isWhite)
        {
            setIsWhite(isWhite);
            setName(name);
        }
        public string getName() { return name; }
        public bool getIsWhite() { return isWhite; }
        public void setName(string name)
        {
            this.name = (isWhite ? "W" : "B")+ name;
        }
        public void setIsWhite(bool isWhite)
        {
            this.isWhite = isWhite;
        }
        public virtual bool isPotentialMove(Piece[,] currentPosition,int beginRowSquare, int beginColumnSquare, int endRowSquare, int endColumnSquare)
        { return false; }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            return ((Piece)obj).name == this.name;
        }
        public override string ToString()
        {
            return name;
        }
    }
    class Rook : Piece
    {
        bool alreadyMove;
        public Rook(bool isWhite) : base("R", isWhite)
        {
            setAlreadyMove(false);
        }
        public void setAlreadyMove(bool alreadyMove)
        {
            this.alreadyMove = alreadyMove;
        }
        public bool getAlreadyMove()
        {
            return alreadyMove;
        }
        public override bool isPotentialMove(Piece[,] currentPosition, int beginRowSquare, int beginColumnSquare, int endRowSquare, int endColumnSquare)
        {         
            int direction,rowOrColumn;
            //up or down
            if (beginRowSquare != endRowSquare&& beginColumnSquare == endColumnSquare)
            {
                direction = beginRowSquare < endRowSquare ? 1 : -1;
                rowOrColumn = beginRowSquare+direction;
                while (rowOrColumn != endRowSquare&& currentPosition[rowOrColumn, beginColumnSquare] == null)
                    rowOrColumn += direction;
                
                if ((rowOrColumn==endRowSquare)&&(currentPosition[rowOrColumn,beginColumnSquare] == null || currentPosition[rowOrColumn,beginColumnSquare].getIsWhite() != this.getIsWhite()))
                    return true;
            }
            //left or right
            else if (beginColumnSquare != endColumnSquare&& beginRowSquare == endRowSquare)
            {
                direction = beginColumnSquare < endColumnSquare ? 1 : -1;
                rowOrColumn = beginColumnSquare+direction;
                while (rowOrColumn != endColumnSquare&& currentPosition[beginRowSquare, rowOrColumn] == null)
                    rowOrColumn += direction;
              
                if ((rowOrColumn == endColumnSquare) && (currentPosition[beginRowSquare,rowOrColumn] == null || currentPosition[beginRowSquare,rowOrColumn].getIsWhite() != this.getIsWhite()))
                    return true;
            }
            return false;   
        }

    }
    class Knight : Piece
    {
        public Knight(bool isWhite) : base("N", isWhite) { }
        public override bool isPotentialMove(Piece[,] currentPosition, int beginRowSquare, int beginColumnSquare, int endRowSquare, int endColumnSquare)
        {
            if (currentPosition[endRowSquare, endColumnSquare] != null &&currentPosition[endRowSquare, endColumnSquare].getIsWhite() == this.getIsWhite())
                return false;
            int positiveDifferenceRow = endRowSquare - beginRowSquare;
            if (positiveDifferenceRow < 0)
                positiveDifferenceRow = -positiveDifferenceRow;
            int positiveDifferenceColumn = endColumnSquare - beginColumnSquare;
            if (positiveDifferenceColumn < 0)
                positiveDifferenceColumn = -positiveDifferenceColumn;
            if (positiveDifferenceColumn==2&&positiveDifferenceRow==1)
                return true;
            if (positiveDifferenceColumn == 1 && positiveDifferenceRow == 2)
                return true;
            return false;    
        }
    }
    class Bishop : Piece
    {
        public Bishop(bool isWhite) : base("B", isWhite) { }
        public override bool isPotentialMove(Piece[,] currentPosition, int beginRowSquare, int beginColumnSquare, int endRowSquare, int endColumnSquare)
        {
            if (beginColumnSquare == endColumnSquare || beginRowSquare == endRowSquare)
                return false;
            int upOrDown=beginRowSquare - endRowSquare>0?-1:1;
            int rightOrLeft = beginColumnSquare - endColumnSquare > 0 ? -1 : 1;
            int row = beginRowSquare+upOrDown;
            int column = beginColumnSquare+rightOrLeft;
            while (row != endRowSquare && column != endColumnSquare && row < 8 && column < 8 && row >= 0 && column >= 0 && currentPosition[row,column]==null)
            { 
                row += upOrDown;
                column += rightOrLeft;
            }
            if ((row==endRowSquare&&column==endColumnSquare)&&(currentPosition[row, column] == null|| currentPosition[row, column].getIsWhite() != this.getIsWhite()))
                return true;
            return false;
        }
    }
    class Queen : Piece
    {
        public Queen(bool isWhite) : base("Q", isWhite) { }
        public override bool isPotentialMove(Piece[,] currentPosition, int beginRowSquare, int beginColumnSquare, int endRowSquare, int endColumnSquare)
        {
            Piece bishop = new Bishop(getIsWhite());
            Piece rook = new Rook(getIsWhite());
            if (bishop.isPotentialMove(currentPosition, beginRowSquare, beginColumnSquare, endRowSquare, endColumnSquare))
                return true;
            if (rook.isPotentialMove(currentPosition, beginRowSquare, beginColumnSquare, endRowSquare, endColumnSquare))
                return true;
            return false;

        }
    }
    class King : Piece
    {
        bool alreadyMove;
        public King(bool isWhite) : base("K", isWhite)
        {
            setAlreadyMove(false);
        }
        public void setAlreadyMove(bool alreadyMove)
        {
            this.alreadyMove = alreadyMove;
        }
        public bool getAlreadyMove()
        {
            return alreadyMove;
        }
        public override bool isPotentialMove(Piece[,] currentPosition, int beginRowSquare, int beginColumnSquare, int endRowSquare, int endColumnSquare)
        {
            int positiveDifferenceRow = endRowSquare - beginRowSquare;
            if (positiveDifferenceRow < 0)
                positiveDifferenceRow = -positiveDifferenceRow;
            int positiveDifferenceColumn = endColumnSquare - beginColumnSquare;
            if (positiveDifferenceColumn < 0)
                positiveDifferenceColumn = -positiveDifferenceColumn;
            if ((positiveDifferenceColumn==1&&positiveDifferenceRow==1)|| (positiveDifferenceColumn==1&&positiveDifferenceRow==0)||(positiveDifferenceColumn == 0 && positiveDifferenceRow == 1))
                if (currentPosition[endRowSquare, endColumnSquare] == null || currentPosition[endRowSquare, endColumnSquare].getIsWhite() != this.getIsWhite())
                    return true;
            return false;
        }


    }
    class Pawn : Piece
    {
        public Pawn(bool isWhite) : base("P", isWhite) { }
        public override bool isPotentialMove(Piece[,] currentPosition, int beginRowSquare, int beginColumnSquare, int endRowSquare, int endColumnSquare)
        {
            int startRow = getIsWhite() ? 1 : 6;
            int upOrDown = getIsWhite() ? 1 : -1;
            if (beginColumnSquare == endColumnSquare)
            {
                if (endRowSquare - beginRowSquare == upOrDown && currentPosition[endRowSquare, endColumnSquare] == null)
                    return true;
                if (endRowSquare - beginRowSquare == 2 * upOrDown && beginRowSquare == startRow)
                    if (currentPosition[beginRowSquare + upOrDown, endColumnSquare] == null)
                        if (currentPosition[beginRowSquare + 2 * upOrDown, endColumnSquare] == null)
                            return true;
            }
            else
            {
                if (endColumnSquare - beginColumnSquare != 1 && endColumnSquare - beginColumnSquare != -1)
                    return false;
                if (endRowSquare - beginRowSquare == upOrDown)
                {
                    if (currentPosition[endRowSquare, endColumnSquare] != null && currentPosition[endRowSquare, endColumnSquare].getIsWhite() != this.getIsWhite())
                        return true;
                }
            }
            return false;
        }

    }
}