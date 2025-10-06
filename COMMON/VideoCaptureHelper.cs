using System;
using System.IO;
using OpenCvSharp;

using System;
using System.IO;
using OpenCvSharp;

namespace COMMON
{
    public static class VideoCaptureHelper
    {
        
        public static bool TryCaptureRandomFrame(string videoPath, string imagePath, out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(videoPath) || !File.Exists(videoPath))
            {
                message = "Invalid video path or file does not exist.";
                return false;
            }

            try
            {
                using (var capture = new VideoCapture(videoPath))
                {
                    if (!capture.IsOpened())
                    {
                        message = "Unable to open the video file.";
                        return false;
                    }

                    // Retrieve FPS and total frame count via VideoCaptureProperties
                    double fps = capture.Get(VideoCaptureProperties.Fps);
                    double frameCount = capture.Get(VideoCaptureProperties.FrameCount);

                    if (fps <= 0 || frameCount <= 0)
                    {
                        message = "Unable to retrieve video duration or frame count.";
                        return false;
                    }

                    // Convert total frames to milliseconds
                    double totalDurationMs = (frameCount / fps) * 1000.0;

                    const int maxAttempts = 10;
                    Random random = new Random();
                    Mat? frame = null;

                    bool gotValidFrame = false;
                    for (int i = 0; i < maxAttempts; i++)
                    {
                        double randomMs = random.NextDouble() * totalDurationMs;
                        
                        // Move to the specified time (milliseconds)
                        capture.Set(VideoCaptureProperties.PosMsec, randomMs);

                        frame = new Mat();
                        capture.Read(frame);
                        if (frame.Empty())
                            continue;

                        // Skip pure color frames based on standard deviation
                        if (IsPureColor(frame))
                            continue;

                        gotValidFrame = true;
                        break;
                    }

                    if (!gotValidFrame || frame == null)
                    {
                        message = "Unable to get a valid frame after multiple attempts.";
                        return false;
                    }

                    // Crop to 16:9 aspect ratio
                    Mat cropped = CropTo16x9(frame);

                    try
                    {
                        // Ensure the directory exists
                        var dir = Path.GetDirectoryName(imagePath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        // Save the image
                        cropped.SaveImage(imagePath);
                    }
                    catch (Exception e)
                    {
                        message = "Failed to save image: " + e.Message;
                        return false;
                    }

                    message = "Frame captured and image saved successfully.";
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = "Exception: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Checks if the image is considered "pure color" based on standard deviation.
        /// </summary>
        private static bool IsPureColor(Mat image)
        {
            Cv2.MeanStdDev(image, out Scalar mean, out Scalar stddev);
            const double threshold = 10.0;

            if (stddev.Val0 < threshold && stddev.Val1 < threshold && stddev.Val2 < threshold)
                return true;

            return false;
        }

        /// <summary>
        /// Crops the image to a 16:9 aspect ratio from the center.
        /// </summary>
        private static Mat CropTo16x9(Mat src)
        {
            int width = src.Width;
            int height = src.Height;

            double targetRatio = 16.0 / 9.0;
            double originalRatio = (double)width / height;

            int newWidth, newHeight;

            if (originalRatio > targetRatio)
            {
                newHeight = height;
                newWidth = (int)(height * targetRatio);
            }
            else
            {
                newWidth = width;
                newHeight = (int)(width / targetRatio);
            }

            int x = (width - newWidth) / 2;
            int y = (height - newHeight) / 2;

            Rect roi = new Rect(x, y, newWidth, newHeight);
            Mat cropped = new Mat(src, roi);

            // Optional: resize if you want a specific resolution (e.g., 1280x720)
            // Mat result = cropped.Resize(new Size(1280, 720));
            // return result;

            return cropped;
        }
    }
}

