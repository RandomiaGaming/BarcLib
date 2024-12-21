using System;
using System.Collections.Generic;
using System.Drawing;

namespace BarcLib
{
    public enum Code128Charset
    {
        A,
        B,
        C
    }
    public class Code128BarcodeGenerator
    {
        #region Private Variables
        private int _currentChecksum = 0;
        private bool _initializedBarcode = false;
        private bool _terminatedBarcode = false;
        private Code128Charset? _currentCharset = null;
        private List<Code128Symbol> _barcodeContents = new List<Code128Symbol>();
        #endregion
        #region Public Variables
        //True: Automatically add charset switches, shifts, and starts as needed for most efficient encoding.
        //False: Never add charset switches, shifts, or starts unless instructed to do so manually.
        public bool AutoCharset = true;
        //True: Automatically compute and append checksum and terminator when generating barcode.
        //False: Leave barcode contents exactly as is when generating barcode. Checksum and terminator must be added manually.
        public bool AutoTerminate = true;
        #endregion
        #region Public Constructors
        public Code128BarcodeGenerator() { }
        #endregion
        #region Public Methods
        public void Append(string data)
        {
            if (!_initializedBarcode)
            {
                _barcodeContents.Add(QuietZone);

                _barcodeContents.Add(StartB);

                _currentChecksum = (_currentChecksum + (1 * 104)) % 103; // StartB

                _initializedBarcode = true;
            }

            for (int i = 0; i < data.Length; i++)
            {
                string c = data[i].ToString();
                for (int j = 0; j < LookupTable.Length; j++)
                {
                    Code128Symbol symbol = LookupTable[j];
                    if (symbol.ValueB == c)
                    {
                        _currentChecksum = (_currentChecksum + (j * (_barcodeContents.Count - 1))) % 103;

                        _barcodeContents.Add(symbol);
                        
                        break;
                    }
                }
            }
        }
        public void SwitchCharsets(Code128Charset newCharset)
        {

        }
        public void Terminate()
        {
            if (_terminatedBarcode)
            {
                return;
            }

            _barcodeContents.Add(LookupTable[_currentChecksum]);

            _barcodeContents.Add(Stop);
            _barcodeContents.Add(FinalBar);
            _barcodeContents.Add(QuietZone);

            _terminatedBarcode = true;
        }
        public Bitmap Generate()
        {
            return Generate(10);
        }
        public Bitmap Generate(int height)
        {
            return Generate(height, 1);
        }
        public Bitmap Generate(int height, int stride)
        {
            return Generate(height, stride, Color.White, Color.Black);
        }
        public Bitmap Generate(int height, int stride, Color lowColor, Color highColor)
        {
            if (AutoTerminate)
            {
                Terminate();
            }

            // Calculate width
            int width = 0;
            for (int i = 0; i < _barcodeContents.Count; i++)
            {
                Code128Symbol symbol = _barcodeContents[i];
                width += symbol.BarWidths[0] + symbol.BarWidths[1] + symbol.BarWidths[2] + symbol.BarWidths[3] + symbol.BarWidths[4] + symbol.BarWidths[5];
            }
            width *= stride;

            // Allocate bitmap
            Bitmap output = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(output);
            SolidBrush lowBrush = new SolidBrush(lowColor);
            SolidBrush highBrush = new SolidBrush(highColor);

            // Render
            int offset = 0;
            for (int i = 0; i < _barcodeContents.Count; i++)
            {
                Code128Symbol symbol = _barcodeContents[i];

                int barWidth = symbol.BarWidths[0] * stride;
                graphics.FillRectangle(highBrush, new Rectangle(offset, 0, barWidth, height));
                offset += barWidth;

                barWidth = symbol.BarWidths[1] * stride;
                graphics.FillRectangle(lowBrush, new Rectangle(offset, 0, barWidth, height));
                offset += barWidth;

                barWidth = symbol.BarWidths[2] * stride;
                graphics.FillRectangle(highBrush, new Rectangle(offset, 0, barWidth, height));
                offset += barWidth;

                barWidth = symbol.BarWidths[3] * stride;
                graphics.FillRectangle(lowBrush, new Rectangle(offset, 0, barWidth, height));
                offset += barWidth;

                barWidth = symbol.BarWidths[4] * stride;
                graphics.FillRectangle(highBrush, new Rectangle(offset, 0, barWidth, height));
                offset += barWidth;

                barWidth = symbol.BarWidths[5] * stride;
                graphics.FillRectangle(lowBrush, new Rectangle(offset, 0, barWidth, height));
                offset += barWidth;
            }

            // Clean up and return
            lowBrush.Dispose();
            highBrush.Dispose();
            graphics.Dispose();

            return output;
        }
        #endregion
        #region Private Subclasses
        public sealed class Code128Symbol
        {
            // All bar width sequences must start with a black bar aka a 1 and must end with a white bar aka a 0.
            // This means that the sequence 00000000000 would be represented as new int[] { 0, 11 }.
            // This means a 0 width black bar and an 11 width white bar which is legal.
            public readonly int[] BarWidths;
            public readonly string ValueA;
            public readonly string ValueB;
            public readonly string ValueC;
            public Code128Symbol(int[] barWidths, string valueA, string valueB, string valueC)
            {
                BarWidths = barWidths;
                ValueA = valueA;
                ValueB = valueB;
                ValueC = valueC;
            }
            public override string ToString()
            {
                return $"{ValueA} {ValueB} {ValueC}";
            }
        }
        #endregion
        #region Private Static Database
        public static readonly Code128Symbol[] LookupTable = new Code128Symbol[]
        {
             new Code128Symbol(new int[] { 2, 1, 2, 2, 2, 2 }, " ", " ", "0"),
             new Code128Symbol(new int[] { 2, 2, 2, 1, 2, 2 }, "!", "!", "1"),
             new Code128Symbol(new int[] { 2, 2, 2, 2, 2, 1 }, "\"", "\"", "2"),
             new Code128Symbol(new int[] { 1, 2, 1, 2, 2, 3 }, "##", "##", "3"),
             new Code128Symbol(new int[] { 1, 2, 1, 3, 2, 2 }, "$", "$", "4"),
             new Code128Symbol(new int[] { 1, 3, 1, 2, 2, 2 }, "%", "%", "5"),
             new Code128Symbol(new int[] { 1, 2, 2, 2, 1, 3 }, "&", "&", "6"),
             new Code128Symbol(new int[] { 1, 2, 2, 3, 1, 2 }, "\'", "\'", "7"),
             new Code128Symbol(new int[] { 1, 3, 2, 2, 1, 2 }, "(", "(", "8"),
             new Code128Symbol(new int[] { 2, 2, 1, 2, 1, 3 }, ")", ")", "9"),
             new Code128Symbol(new int[] { 2, 2, 1, 3, 1, 2 }, "*", "*", "10"),
             new Code128Symbol(new int[] { 2, 3, 1, 2, 1, 2 }, "+", "+", "11"),
             new Code128Symbol(new int[] { 1, 1, 2, 2, 3, 2 }, ",", ",", "12"),
             new Code128Symbol(new int[] { 1, 2, 2, 1, 3, 2 }, "-", "-", "13"),
             new Code128Symbol(new int[] { 1, 2, 2, 2, 3, 1 }, ".", ".", "14"),
             new Code128Symbol(new int[] { 1, 1, 3, 2, 2, 2 }, "/", "/", "15"),
             new Code128Symbol(new int[] { 1, 2, 3, 1, 2, 2 }, "0", "0", "16"),
             new Code128Symbol(new int[] { 1, 2, 3, 2, 2, 1 }, "1", "1", "17"),
             new Code128Symbol(new int[] { 2, 2, 3, 2, 1, 1 }, "2", "2", "18"),
             new Code128Symbol(new int[] { 2, 2, 1, 1, 3, 2 }, "3", "3", "19"),
             new Code128Symbol(new int[] { 2, 2, 1, 2, 3, 1 }, "4", "4", "20"),
             new Code128Symbol(new int[] { 2, 1, 3, 2, 1, 2 }, "5", "5", "21"),
             new Code128Symbol(new int[] { 2, 2, 3, 1, 1, 2 }, "6", "6", "22"),
             new Code128Symbol(new int[] { 3, 1, 2, 1, 3, 1 }, "7", "7", "23"),
             new Code128Symbol(new int[] { 3, 1, 1, 2, 2, 2 }, "8", "8", "24"),
             new Code128Symbol(new int[] { 3, 2, 1, 1, 2, 2 }, "9", "9", "25"),
             new Code128Symbol(new int[] { 3, 2, 1, 2, 2, 1 }, ":", ":", "26"),
             new Code128Symbol(new int[] { 3, 1, 2, 2, 1, 2 }, ";", ";", "27"),
             new Code128Symbol(new int[] { 3, 2, 2, 1, 1, 2 }, "<", "<", "28"),
             new Code128Symbol(new int[] { 3, 2, 2, 2, 1, 1 }, "=", "=", "29"),
             new Code128Symbol(new int[] { 2, 1, 2, 1, 2, 3 }, ">", ">", "30"),
             new Code128Symbol(new int[] { 2, 1, 2, 3, 2, 1 }, "?", "?", "31"),
             new Code128Symbol(new int[] { 2, 3, 2, 1, 2, 1 }, "@", "@", "32"),
             new Code128Symbol(new int[] { 1, 1, 1, 3, 2, 3 }, "A", "A", "33"),
             new Code128Symbol(new int[] { 1, 3, 1, 1, 2, 3 }, "B", "B", "34"),
             new Code128Symbol(new int[] { 1, 3, 1, 3, 2, 1 }, "C", "C", "35"),
             new Code128Symbol(new int[] { 1, 1, 2, 3, 1, 3 }, "D", "D", "36"),
             new Code128Symbol(new int[] { 1, 3, 2, 1, 1, 3 }, "E", "E", "37"),
             new Code128Symbol(new int[] { 1, 3, 2, 3, 1, 1 }, "F", "F", "38"),
             new Code128Symbol(new int[] { 2, 1, 1, 3, 1, 3 }, "G", "G", "39"),
             new Code128Symbol(new int[] { 2, 3, 1, 1, 1, 3 }, "H", "H", "40"),
             new Code128Symbol(new int[] { 2, 3, 1, 3, 1, 1 }, "I", "I", "41"),
             new Code128Symbol(new int[] { 1, 1, 2, 1, 3, 3 }, "J", "J", "42"),
             new Code128Symbol(new int[] { 1, 1, 2, 3, 3, 1 }, "K", "K", "43"),
             new Code128Symbol(new int[] { 1, 3, 2, 1, 3, 1 }, "L", "L", "44"),
             new Code128Symbol(new int[] { 1, 1, 3, 1, 2, 3 }, "M", "M", "45"),
             new Code128Symbol(new int[] { 1, 1, 3, 3, 2, 1 }, "N", "N", "46"),
             new Code128Symbol(new int[] { 1, 3, 3, 1, 2, 1 }, "O", "O", "47"),
             new Code128Symbol(new int[] { 3, 1, 3, 1, 2, 1 }, "P", "P", "48"),
             new Code128Symbol(new int[] { 2, 1, 1, 3, 3, 1 }, "Q", "Q", "49"),
             new Code128Symbol(new int[] { 2, 3, 1, 1, 3, 1 }, "R", "R", "50"),
             new Code128Symbol(new int[] { 2, 1, 3, 1, 1, 3 }, "S", "S", "51"),
             new Code128Symbol(new int[] { 2, 1, 3, 3, 1, 1 }, "T", "T", "52"),
             new Code128Symbol(new int[] { 2, 1, 3, 1, 3, 1 }, "U", "U", "53"),
             new Code128Symbol(new int[] { 3, 1, 1, 1, 2, 3 }, "V", "V", "54"),
             new Code128Symbol(new int[] { 3, 1, 1, 3, 2, 1 }, "W", "W", "55"),
             new Code128Symbol(new int[] { 3, 3, 1, 1, 2, 1 }, "X", "X", "56"),
             new Code128Symbol(new int[] { 3, 1, 2, 1, 1, 3 }, "Y", "Y", "57"),
             new Code128Symbol(new int[] { 3, 1, 2, 3, 1, 1 }, "Z", "Z", "58"),
             new Code128Symbol(new int[] { 3, 3, 2, 1, 1, 1 }, "[", "[", "59"),
             new Code128Symbol(new int[] { 3, 1, 4, 1, 1, 1 }, "\\", "\\", "60"),
             new Code128Symbol(new int[] { 2, 2, 1, 4, 1, 1 }, "]", "]", "61"),
             new Code128Symbol(new int[] { 4, 3, 1, 1, 1, 1 }, "^", "^", "62"),
             new Code128Symbol(new int[] { 1, 1, 1, 2, 2, 4 }, "_", "_", "63"),
             new Code128Symbol(new int[] { 1, 1, 1, 4, 2, 2 }, "\u0000", "`", "64"),
             new Code128Symbol(new int[] { 1, 2, 1, 1, 2, 4 }, "\u0001", "a", "65"),
             new Code128Symbol(new int[] { 1, 2, 1, 4, 2, 1 }, "\u0002", "b", "66"),
             new Code128Symbol(new int[] { 1, 4, 1, 1, 2, 2 }, "\u0003", "c", "67"),
             new Code128Symbol(new int[] { 1, 4, 1, 2, 2, 1 }, "\u0004", "d", "68"),
             new Code128Symbol(new int[] { 1, 1, 2, 2, 1, 4 }, "\u0005", "e", "69"),
             new Code128Symbol(new int[] { 1, 1, 2, 4, 1, 2 }, "\u0006", "f", "70"),
             new Code128Symbol(new int[] { 1, 2, 2, 1, 1, 4 }, "\u0007", "g", "71"),
             new Code128Symbol(new int[] { 1, 2, 2, 4, 1, 1 }, "\u0008", "h", "72"),
             new Code128Symbol(new int[] { 1, 4, 2, 1, 1, 2 }, "\u0009", "i", "73"),
             new Code128Symbol(new int[] { 1, 4, 2, 2, 1, 1 }, "\u000A", "j", "74"),
             new Code128Symbol(new int[] { 2, 4, 1, 2, 1, 1 }, "\u000B", "k", "75"),
             new Code128Symbol(new int[] { 2, 2, 1, 1, 1, 4 }, "\u000C", "l", "76"),
             new Code128Symbol(new int[] { 4, 1, 3, 1, 1, 1 }, "\u000D", "m", "77"),
             new Code128Symbol(new int[] { 2, 4, 1, 1, 1, 2 }, "\u000E", "n", "78"),
             new Code128Symbol(new int[] { 1, 3, 4, 1, 1, 1 }, "\u000F", "o", "79"),
             new Code128Symbol(new int[] { 1, 1, 1, 2, 4, 2 }, "\u0010", "p", "80"),
             new Code128Symbol(new int[] { 1, 2, 1, 1, 4, 2 }, "\u0011", "q", "81"),
             new Code128Symbol(new int[] { 1, 2, 1, 2, 4, 1 }, "\u0012", "r", "82"),
             new Code128Symbol(new int[] { 1, 1, 4, 2, 1, 2 }, "\u0013", "s", "83"),
             new Code128Symbol(new int[] { 1, 2, 4, 1, 1, 2 }, "\u0014", "t", "84"),
             new Code128Symbol(new int[] { 1, 2, 4, 2, 1, 1 }, "\u0015", "u", "85"),
             new Code128Symbol(new int[] { 4, 1, 1, 2, 1, 2 }, "\u0016", "v", "86"),
             new Code128Symbol(new int[] { 4, 2, 1, 1, 1, 2 }, "\u0017", "w", "87"),
             new Code128Symbol(new int[] { 4, 2, 1, 2, 1, 1 }, "\u0018", "x", "88"),
             new Code128Symbol(new int[] { 2, 1, 2, 1, 4, 1 }, "\u0019", "y", "89"),
             new Code128Symbol(new int[] { 2, 1, 4, 1, 2, 1 }, "\u001A", "z", "90"),
             new Code128Symbol(new int[] { 4, 1, 2, 1, 2, 1 }, "\u001B", "{", "91"),
             new Code128Symbol(new int[] { 1, 1, 1, 1, 4, 3 }, "\u001C", "|", "92"),
             new Code128Symbol(new int[] { 1, 1, 1, 3, 4, 1 }, "\u001D", "}", "93"),
             new Code128Symbol(new int[] { 1, 3, 1, 1, 4, 1 }, "\u001E", "~", "94"),
             new Code128Symbol(new int[] { 1, 1, 4, 1, 1, 3 }, "\u001F", "\u007F", "95"),
             new Code128Symbol(new int[] { 1, 1, 4, 3, 1, 1 }, "#FNC3", "#FNC3", "96"),
             new Code128Symbol(new int[] { 4, 1, 1, 1, 1, 3 }, "#FNC2", "#FNC2", "97"),
             new Code128Symbol(new int[] { 4, 1, 1, 3, 1, 1 }, "#ShiftB", "#ShiftA", "98"),
             new Code128Symbol(new int[] { 1, 1, 3, 1, 4, 1 }, "#CodeC", "#CodeC", "99"),
             new Code128Symbol(new int[] { 1, 1, 4, 1, 3, 1 }, "#CodeB", "#FNC4", "#CodeB"),
             new Code128Symbol(new int[] { 3, 1, 1, 1, 4, 1 }, "#FNC4", "#CodeA", "#CodeA"),
             new Code128Symbol(new int[] { 4, 1, 1, 1, 3, 1 }, "#FNC1", "#FNC1", "#FNC1"),
             new Code128Symbol(new int[] { 2, 1, 1, 4, 1, 2 }, "#StartA", "#StartA", "#StartA"),
             new Code128Symbol(new int[] { 2, 1, 1, 2, 1, 4 }, "#StartB", "#StartB", "#StartB"),
             new Code128Symbol(new int[] { 2, 1, 1, 2, 3, 2 }, "#StartC", "#StartC", "#StartC"),
             new Code128Symbol(new int[] { 2, 3, 3, 1, 1, 1 }, "#Stop", "#Stop", "#Stop"),
             new Code128Symbol(new int[] { 2, 0, 0, 0, 0, 0 }, "#FinalBar", "#FinalBar", "#FinalBar"),
             new Code128Symbol(new int[] { 0, 10, 0, 0, 0, 0 }, "#QuietZone", "#QuietZone", "#QuietZone"),
        };
        public static readonly Code128Symbol StartA = LookupTable[103];
        public static readonly Code128Symbol StartB = LookupTable[104];
        public static readonly Code128Symbol StartC = LookupTable[105];
        public static readonly Code128Symbol Stop = LookupTable[106];
        public static readonly Code128Symbol FinalBar = LookupTable[107];
        public static readonly Code128Symbol QuietZone = LookupTable[108];
        #endregion
    }
}
