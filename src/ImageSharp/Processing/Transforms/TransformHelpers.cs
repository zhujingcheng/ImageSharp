﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using SixLabors.ImageSharp.MetaData.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace SixLabors.ImageSharp
{
    /// <summary>
    /// Contains helper methods for working with affine and non-affine transforms
    /// </summary>
    internal class TransformHelpers
    {
        /// <summary>
        /// Updates the dimensional metadata of a transformed image
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="image">The image to update</param>
        public static void UpdateDimensionalMetData<TPixel>(Image<TPixel> image)
            where TPixel : struct, IPixel<TPixel>
        {
            ExifProfile profile = image.MetaData.ExifProfile;
            if (profile == null)
            {
                return;
            }

            // Removing the previously stored value allows us to set a value with our own data tag if required.
            if (profile.GetValue(ExifTag.PixelXDimension) != null)
            {
                profile.RemoveValue(ExifTag.PixelXDimension);

                if (image.Width <= ushort.MaxValue)
                {
                    profile.SetValue(ExifTag.PixelXDimension, (ushort)image.Width);
                }
                else
                {
                    profile.SetValue(ExifTag.PixelXDimension, (uint)image.Width);
                }
            }

            if (profile.GetValue(ExifTag.PixelYDimension) != null)
            {
                profile.RemoveValue(ExifTag.PixelYDimension);

                if (image.Height <= ushort.MaxValue)
                {
                    profile.SetValue(ExifTag.PixelYDimension, (ushort)image.Height);
                }
                else
                {
                    profile.SetValue(ExifTag.PixelYDimension, (uint)image.Height);
                }
            }
        }

        /// <summary>
        /// Returns the bounding <see cref="Rectangle"/> relative to the source for the given transformation matrix.
        /// </summary>
        /// <param name="rectangle">The source rectangle.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>
        /// The <see cref="Rectangle"/>.
        /// </returns>
        public static Rectangle GetTransformedBoundingRectangle(Rectangle rectangle, Matrix3x2 matrix)
        {
            // Calculate the position of the four corners in world space by applying
            // The world matrix to the four corners in object space (0, 0, width, height)
            var tl = Vector2.Transform(Vector2.Zero, matrix);
            var tr = Vector2.Transform(new Vector2(rectangle.Width, 0), matrix);
            var bl = Vector2.Transform(new Vector2(0, rectangle.Height), matrix);
            var br = Vector2.Transform(new Vector2(rectangle.Width, rectangle.Height), matrix);

            return GetBoundingRectangle(tl, tr, bl, br);
        }

        /// <summary>
        /// Returns the bounding <see cref="Rectangle"/> relative to the source for the given transformation matrix.
        /// </summary>
        /// <param name="rectangle">The source rectangle.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>
        /// The <see cref="Rectangle"/>.
        /// </returns>
        public static Rectangle GetTransformedBoundingRectangle(Rectangle rectangle, Matrix4x4 matrix)
        {
            // Calculate the position of the four corners in world space by applying
            // The world matrix to the four corners in object space (0, 0, width, height)
            var tl = Vector2.Transform(Vector2.Zero, matrix);
            var tr = Vector2.Transform(new Vector2(rectangle.Width, 0), matrix);
            var bl = Vector2.Transform(new Vector2(0, rectangle.Height), matrix);
            var br = Vector2.Transform(new Vector2(rectangle.Width, rectangle.Height), matrix);

            return GetBoundingRectangle(tl, tr, bl, br);
        }

        private static Rectangle GetBoundingRectangle(Vector2 tl, Vector2 tr, Vector2 bl, Vector2 br)
        {
            // Find the minimum and maximum "corners" based on the given vectors
            float minX = MathF.Min(tl.X, MathF.Min(tr.X, MathF.Min(bl.X, br.X)));
            float maxX = MathF.Max(tl.X, MathF.Max(tr.X, MathF.Max(bl.X, br.X)));
            float minY = MathF.Min(tl.Y, MathF.Min(tr.Y, MathF.Min(bl.Y, br.Y)));
            float maxY = MathF.Max(tl.Y, MathF.Max(tr.Y, MathF.Max(bl.Y, br.Y)));
            float sizeX = maxX - minX + .5F;
            float sizeY = maxY - minY + .5F;

            return new Rectangle((int)(MathF.Ceiling(minX) - .5F), (int)(MathF.Ceiling(minY) - .5F), (int)MathF.Floor(sizeX), (int)MathF.Floor(sizeY));
        }
    }
}