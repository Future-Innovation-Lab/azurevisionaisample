using Azure;
using Azure.AI.Vision.ImageAnalysis;
using SkiaSharp;
using System;
using System.IO;
using System.Reflection.Metadata;

public class Program
{
    static void AnalyzeImage()
    {
        string key = "<Azure Vision Key>";
        string endpoint = "<Azure Vision Endpoint>";

        ImageAnalysisClient client = new ImageAnalysisClient(
            new Uri(endpoint),
            new AzureKeyCredential(key));

        //  Image from File Sample

        using FileStream stream = new FileStream("<your image file>", FileMode.Open);
        BinaryData imageData = BinaryData.FromStream(stream);


        //  Image from URL Sample
        /*  ImageAnalysisResult result = client.Analyze(
              new Uri("https://learn.microsoft.com/azure/ai-services/computer-vision/media/quickstarts/presentation.png"),
              VisualFeatures.Caption | VisualFeatures.Read,
              new ImageAnalysisOptions { GenderNeutralCaption = true });*/

        ImageAnalysisResult result = client.Analyze(imageData,
                    VisualFeatures.Caption | VisualFeatures.Read,
                    new ImageAnalysisOptions { GenderNeutralCaption = true });


        Console.WriteLine("Image analysis results:");
        Console.WriteLine(" Caption:");
        Console.WriteLine($"   '{result.Caption.Text}', Confidence {result.Caption.Confidence:F4}");


        //using skiaSharp to draw bounding boxes on an image

        var image = SKImage.FromEncodedData(@"<your image file>");
        SKBitmap bmp = SKBitmap.FromImage(image);
        SKCanvas canvas = new(bmp);

        SKColor boundingBoxColor = new SKColor(255, 0, 0);

        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Red,
            StrokeWidth = 2
        };


        Console.WriteLine(" Read:");
        foreach (DetectedTextBlock block in result.Read.Blocks)
            foreach (DetectedTextLine line in block.Lines)
            {
                Console.WriteLine($"   Line: '{line.Text}', Bounding Polygon: [{string.Join(" ", line.BoundingPolygon)}]");

                using (SKPath path = new SKPath())
                {
                    for (int index = 0; index < line.BoundingPolygon.Count; index++)
                    {
                        SKPoint skPoint = new SKPoint() { X = line.BoundingPolygon[index].X, Y = line.BoundingPolygon[index].Y };

                        if (index == 0)
                        {
                            path.MoveTo(skPoint);
                        }
                        else
                        {
                            path.LineTo(skPoint);
                        }
                    }

                    path.Close();

                    canvas.DrawPath(path, paint);

                }

                /*                foreach (var boundingPoint in line.BoundingPolygon)
                                {
                                    SKPoint skPoint = new SKPoint() { X = boundingPoint.X, Y = boundingPoint.Y };
                                    canvas.DrawPoint(skPoint, paint);
                                }
                */

                foreach (DetectedTextWord word in line.Words)
                {
                    Console.WriteLine($"     Word: '{word.Text}', Confidence {word.Confidence.ToString("#.####")}, Bounding Polygon: [{string.Join(" ", word.BoundingPolygon)}]");


                    foreach (var boundingPoint in word.BoundingPolygon)
                    {
                        SKPoint skPoint = new SKPoint() { X = boundingPoint.X, Y = boundingPoint.Y };
                        canvas.DrawPoint(skPoint, paint);
                    }
                }
            }

        SKFileWStream fs = new("output.jpg");
        bmp.Encode(fs, SKEncodedImageFormat.Jpeg, quality: 85);
    }

    static void Main()
    {
        try
        {
            AnalyzeImage();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}