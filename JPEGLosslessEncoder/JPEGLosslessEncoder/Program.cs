using System;
using System.Collections.Generic;
using System.IO;

namespace JPEGLosslessEncoder
{
    class Program
    {
        //Use the following Huffman table for entropy encoding
        private static string HuffmanTableEncoder(int key)
        {
            switch (key)
            {
                case 0:
                    return "1";
                case 1:
                    return "00";
                case -1:
                    return "011";
                case 2:
                    return "0100";
                case -2:
                    return "01011";
                case 3:
                    return "010100";
                case -3:
                    return "0101011";
                case 4:
                    return "01010100";
                case -4:
                    return "010101011";
                case 5:
                    return "0101010100";
                case -5:
                    return "01010101011";
                case 6:
                    return "010101010100";
                case -6:
                    return "0101010101011";
                default:
                    return Convert.ToString(key, 2);
            }
        }

        //Use the following Huffman table for entropy decoding
        private static int HuffmanTableDecoder(string b)
        {
            switch (b)
            {
                case "1":
                    return 0;
                case "00":
                    return 1;
                case "011":
                    return -1;
                case "0100":
                    return 2;
                case "01011":
                    return -2;
                case "010100":
                    return 3;
                case "0101011":
                    return -3;
                case "01010100":
                    return 4;
                case "010101011":
                    return -4;
                case "0101010100":
                    return 5;
                case "01010101011":
                    return -5;
                case "010101010100":
                    return 6;
                case "0101010101011":
                    return -6;
                default:
                    var x = b.PadLeft(8, '0');
                    return Convert.ToInt32(x, 2);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                //string path = Directory.GetCurrentDirectory();
                string text = System.IO.File.ReadAllText(@"image.txt").Replace("\r\n", " ");
                var formattedText = text.Split(" ");

                //Use a 2d array to store original image.
                int[,] originalImage = new int[16, 16];

                for (int i = 0; i < formattedText.Length; i++)
                {
                    originalImage[i / 16, i % 16] = int.Parse(formattedText[i]);
                }

                if (originalImage.Length != 256)
                {
                    Console.WriteLine("Program Stopped, bad data. Image must be 16x16.");
                    return;
                }


                for (int i = 1; i < 8; i++)
                {
                    var coefficients = RunEncoderFormula(i, originalImage);

                    RunEncoderAndDecoder(i, coefficients);
                    RunCalculations(i, originalImage, coefficients);

                    Console.WriteLine("________________________________________________________________________________________________");
                    Console.WriteLine(Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running program - {ex.Message}");
            }
        }

        /// <summary>
        /// Returns Encoded array using the index i as the selector for which formula to use for the encoding.
        /// </summary>
        /// <param name="i"></param>
        private static int[,] RunEncoderFormula(int i, int[,] array)
        {
            switch (i)
            {
                case 1:
                    return FirstFormula(array);
                case 2:
                    return SecondFormula(array);
                case 3:
                    return ThirdFormula(array);
                case 4:
                    return FourthFormula(array);
                case 5:
                    return FifthFormula(array);
                case 6:
                    return SixthFormula(array);
                case 7:
                    return SeventhFormula(array);
                default:
                    return null;
            }
        }

        /// <summary>
        /// X = A
        /// First row always X = A, First Column always X = B
        /// </summary>
        /// <param name="array"></param>
        static int[,] FirstFormula(int[,] array)
        {
            Console.WriteLine("Running First Formula X = A.");
            Console.WriteLine("Original 16x16 image:");
            print2DArray(array);

            var coefficients = InitializeCoefficientsArray(array);

            for (int i = 1; i < 16; i++)
            {
                for (int j = 1; j < 16; j++)
                {
                    coefficients[i, j] = DoXEqualsA(array, i, j);
                }
            }

            return coefficients;
        }

        /// <summary>
        /// X = B
        /// First row always X = A, First Column always X = B
        /// </summary>
        /// <param name="array"></param>
        static int[,] SecondFormula(int[,] array)
        {
            Console.WriteLine("Running Second Formula X = B.");
            Console.WriteLine("Original 16x16 image:");
            print2DArray(array);

            var coefficients = InitializeCoefficientsArray(array);

            for (int i = 1; i < 16; i++)
            {
                for (int j = 1; j < 16; j++)
                {
                    coefficients[i, j] = DoXEqualsB(array, i, j);
                }
            }

            return coefficients;
        }

        /// <summary>
        /// X = C
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        static int[,] ThirdFormula(int[,] array)
        {
            Console.WriteLine("Running Third Formula X = C.");
            Console.WriteLine("Original 16x16 image:");
            print2DArray(array);

            int[,] coefficients = InitializeCoefficientsArray(array);

            for (int i = 1; i < 16; i++)
            {
                for (int j = 1; j < 16; j++)
                {
                    coefficients[i, j] = array[i, j] - array[i - 1, j - 1];
                }
            }

            return coefficients;
        }

        /// <summary>
        /// X = A + B - C
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        static int[,] FourthFormula(int[,] array)
        {
            Console.WriteLine("Running Fourth Formula X = A + B - C.");
            Console.WriteLine("Original 16x16 image:");
            print2DArray(array);

            int[,] coefficients = InitializeCoefficientsArray(array);

            for (int i = 1; i < 16; i++)
            {
                for (int j = 1; j < 16; j++)
                {
                    coefficients[i, j] = array[i, j] - (array[i, j - 1] + array[i - 1, j] - array[i - 1, j - 1]);
                }
            }

            return coefficients;
        }

        /// <summary>
        /// X = A + (B-C)/2
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        static int[,] FifthFormula(int[,] array)
        {
            Console.WriteLine("Running Fifth Formula X = A + (B - C)/2.");
            Console.WriteLine("Original 16x16 image:");
            print2DArray(array);

            int[,] coefficients = InitializeCoefficientsArray(array);

            for (int i = 1; i < 16; i++)
            {
                for (int j = 1; j < 16; j++)
                {
                    var x = (int)Math.Ceiling((double)((array[i - 1, j] - array[i - 1, j - 1]) / 2));
                    coefficients[i, j] = array[i, j] - (array[i, j - 1] + x);
                }
            }

            return coefficients;
        }

        /// <summary>
        /// X = B + (A-C)/2
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        static int[,] SixthFormula(int[,] array)
        {
            Console.WriteLine("Running Sixth Formula X = B + (A - C)/2.");
            Console.WriteLine("Original 16x16 image:");
            print2DArray(array);

            int[,] coefficients = InitializeCoefficientsArray(array);

            for (int i = 1; i < 16; i++)
            {
                for (int j = 1; j < 16; j++)
                {
                    var x = (int)Math.Ceiling((double)((array[i, j - 1] - array[i - 1, j - 1]) / 2));
                    coefficients[i, j] = array[i, j] - (array[i - 1, j] + x);
                }
            }

            return coefficients;
        }

        /// <summary>
        /// X = (A + B)/2
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        static int[,] SeventhFormula(int[,] array)
        {
            Console.WriteLine("Running Seventh Formula X = (A + B)/2.");
            Console.WriteLine("Original 16x16 image:");
            print2DArray(array);

            int[,] coefficients = InitializeCoefficientsArray(array);

            for (int i = 1; i < 16; i++)
            {
                for (int j = 1; j < 16; j++)
                {
                    var x = (int)Math.Ceiling((double)((array[i, j - 1] + array[i - 1, j]) / 2));
                    coefficients[i, j] = array[i, j] - x;
                }
            }

            return coefficients;
        }


        /// <summary>
        /// Runs the formula X = A to encode the image
        /// </summary>
        /// <param name="array"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        static int DoXEqualsA(int[,] array, int i, int j)
        {
            return array[i, j] - array[i, j - 1];
        }

        /// <summary>
        /// Runs the formula X = B to encode the image
        /// </summary>
        /// <param name="array"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        static int DoXEqualsB(int[,] array, int i, int j)
        {
            return array[i, j] - array[i - 1, j];
        }

        /// <summary>
        /// Prints The Following:
        /// The coefficients after the predictor
        /// The compressed image in the form of binary sequence.
        /// The image after Huffman decoder
        /// The image after decompression (the sequence of numbers)
        /// </summary>
        /// <param name="coefficients"></param>
        private static void RunEncoderAndDecoder(int i, int[,] coefficients)
        {
            Console.WriteLine("Coefficients after predictor:");
            print2DArray(coefficients);

            Console.WriteLine("Compressed Image as a binary sequence:");
            var encodedArray = EncodeBinarySequence(coefficients);
            print2DArray(encodedArray, GetMaxLengthOfBitsFromImage(encodedArray));

            Console.WriteLine("Image after Huffman Decoder:");
            var decodedArray = DecodeBinarySequence(encodedArray);
            print2DArray(decodedArray);

            Console.WriteLine("Image after Decompression:");
            var decompressedArray = DecompressImage(i, decodedArray);
            print2DArray(decompressedArray);
        }

        /// <summary>
        /// Gets the longest element from the array and returns the length, used for formatting when printing to console.
        /// </summary>
        /// <param name="encodedArray"></param>
        /// <returns></returns>
        private static int GetMaxLengthOfBitsFromImage(string[,] encodedArray)
        {
            int longestBit = 0;

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if (encodedArray[i, j].Length > longestBit)
                        longestBit = encodedArray[i, j].Length;
                }
            }

            return longestBit;
        }

        /// <summary>
        /// Runs these calculations:
        /// Compression ratio
        /// Bits/pixel for the compressed image
        /// RMS Error (Should be zero) 
        /// </summary>
        /// <param name="coefficients"></param>
        private static void RunCalculations(int i, int[,] originalImage, int[,] coefficients)
        {
            var encodedArray = EncodeBinarySequence(coefficients);
            var decodedArray = DecodeBinarySequence(encodedArray);
            var decompressedArray = DecompressImage(i, decodedArray);

            //For Compression Ratio, the number of bits of the original image is always 8x16x16 = 2048
            //Cr = No / Nc = 2048 / Nc
            var compressedBits = GetCompressedBits(encodedArray);
            double Cr = 2048.0 / compressedBits;

            Console.WriteLine($"Compression Ratio = 2048 / {compressedBits} = {Cr.ToString("0.##")}");

            var averageBitsOfCompressedImage = compressedBits / 256;
            Console.WriteLine($"Average Bits/Pixel = {compressedBits} / 256 = {averageBitsOfCompressedImage.ToString("0.##")}");

            double RMSError = GetRMSError(originalImage, decompressedArray);
            Console.WriteLine($"RMS Error = {RMSError}");
        }

        /// <summary>
        /// Gets the RMS error from original image vs decompressed image. Exact formula can be found in report.
        /// </summary>
        /// <param name="originalImage"></param>
        /// <param name="decompressedArray"></param>
        /// <returns></returns>
        private static double GetRMSError(int[,] originalImage, int[,] decompressedArray)
        {
            double rmsValue = 0;

            //Get Sum
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    rmsValue += Math.Pow(originalImage[i,j] - decompressedArray[i,j], 2);
                }
            }

            //Divide by 256, and get Square Root
            rmsValue = Math.Sqrt(rmsValue / 256);

            return rmsValue;
        }

        /// <summary>
        /// Gets total sum of the lenght of the compressed bits of encoded image.
        /// </summary>
        /// <param name="encodedArray"></param>
        /// <returns></returns>
        private static double GetCompressedBits(string[,] encodedArray)
        {
            double total = 0;

            foreach (var pixel in encodedArray)
            {
                total += pixel.Length;
            }

            return total;
        }

        /// <summary>
        /// Encodes the 16x16 image into a binary sequence as described by the Huffman encoder.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        static string[,] EncodeBinarySequence(int[,] array)
        {
            var tempArray = new string[16, 16];

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    tempArray[i, j] = HuffmanTableEncoder(array[i, j]);
                }
            }

            return tempArray;
        }

        /// <summary>
        /// Decodes the 16x16 image into a coefficient sequence as described by the Huffman decoder.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        static int[,] DecodeBinarySequence(string[,] array)
        {
            var tempArray = new int[16, 16];

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    tempArray[i, j] = HuffmanTableDecoder(array[i, j]);
                }
            }

            return tempArray;
        }

        /// <summary>
        /// Decompresses a 16x16 image array by using the coefficients and the first bit.
        /// </summary>
        /// <param name="k">Index of Formula to use for decoding</param>
        /// <param name="decodedArray"></param>
        /// <returns></returns>
        private static int[,] DecompressImage(int k, int[,] decodedArray)
        {
            var tempArray = new int[16, 16];

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        tempArray[i, j] = decodedArray[i, j];
                    }
                    else if (j == 0)
                    {
                        tempArray[i, j] = tempArray[i - 1, j] + decodedArray[i, j];
                    }
                    else
                    {
                        tempArray[i, j] = RunDecompressor(i, j, k, tempArray, decodedArray);
                    }
                }
            }

            return tempArray;
        }

        /// <summary>
        /// Runs what decompression method will be used, we use 'k' as the index to choose which formula was originally used.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        /// <param name="tempArray"></param>
        /// <param name="decodedArray"></param>
        /// <returns></returns>
        private static int RunDecompressor(int i, int j, int k, int[,] tempArray, int[,] decodedArray)
        {
            if (i == 0)
            {
                return tempArray[i, j - 1] + decodedArray[i, j];
            }
            else if (j == 0)
            {
                return tempArray[i - 1, j] + decodedArray[i - 1, j];
            }
            else
            {
                switch (k)
                {
                    case 1:
                        return tempArray[i, j - 1] + decodedArray[i, j];
                    case 2:
                        return tempArray[i - 1, j] + decodedArray[i, j];
                    case 3:
                        return tempArray[i - 1, j - 1] + decodedArray[i, j];
                    case 4:
                        return (tempArray[i, j - 1] + tempArray[i - 1, j] - tempArray[i - 1, j - 1]) + decodedArray[i, j];
                    case 5:
                        var x = (int)Math.Floor((double)((tempArray[i - 1, j] - tempArray[i - 1, j - 1]) / 2));
                        return (tempArray[i, j - 1] + x) + decodedArray[i, j];
                    case 6:
                        var y = (int)Math.Floor((double)((tempArray[i, j - 1] - tempArray[i - 1, j - 1]) / 2));
                        return (tempArray[i - 1, j] + y) + decodedArray[i, j];
                    case 7:
                        var z = (int)Math.Floor((double)((tempArray[i, j - 1] + tempArray[i - 1, j]) / 2));
                        return z + decodedArray[i, j];
                    default:
                        return -1;
                }
            }
        }

        /// <summary>
        /// Initializes an array containing the coefficients for the encoded image. First Row is always X = A, first column is always X = B.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private static int[,] InitializeCoefficientsArray(int[,] array)
        {
            int[,] tempArray = new int[16, 16];

            if (array.Length == 256)
            {
                //Initialize first element
                tempArray[0, 0] = array[0, 0];

                //First Row
                for (int i = 1; i < 16; i++)
                {
                    tempArray[0, i] = DoXEqualsA(array, 0, i);
                }

                //First Column
                for (int j = 1; j < 16; j++)
                {
                    tempArray[j, 0] = DoXEqualsB(array, j, 0);
                }
            }

            return tempArray;
        }

        /// <summary>
        /// Method to print out a 16 x 16 string array to the console neatly.
        /// </summary>
        /// <param name="array"></param>
        static void print2DArray(int[,] array)
        {
            Console.Write(Environment.NewLine);

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    Console.Write(array[i, j].ToString().PadLeft(4));
                }

                Console.Write(Environment.NewLine);
                Console.Write(Environment.NewLine);
            }
        }

        /// <summary>
        /// Method to print out a 16 x 16 int array to the console neatly.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="pad"></param>
        static void print2DArray(string[,] array, int pad = 4)
        {
            Console.Write(Environment.NewLine);

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    Console.Write(array[i, j].PadLeft(pad));
                }

                Console.Write(Environment.NewLine);
                Console.Write(Environment.NewLine);
            }
        }
    }
}
