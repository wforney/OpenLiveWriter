// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// Class WindowCascadeHelper. This class cannot be inherited.
    /// </summary>
    public sealed class WindowCascadeHelper
    {
        /// <summary>
        /// The reference location
        /// </summary>
        private static Point referenceLocation;

        /// <summary>
        /// Cannot be instantiated or subclassed
        /// </summary>
        private WindowCascadeHelper()
        {
        }

        /// <summary>
        /// Sets the next opened location.
        /// </summary>
        /// <param name="location">The location.</param>
        public static void SetNextOpenedLocation(Point location) => referenceLocation = location;

        /// <summary>
        /// Gets the new post location.
        /// </summary>
        /// <param name="formSize">Size of the form.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>Point.</returns>
        public static Point GetNewPostLocation(Size formSize, int offset)
        {
            // case 1: opened through new post, so there is a window to cascade against
            if (!referenceLocation.IsEmpty)
            {
                var openerLocation = referenceLocation;

                // make sure that we will cascade against a visible window
                var targetScreen = FindTitlebarVisibleScreen(openerLocation, formSize, offset);
                if (null != targetScreen)
                {
                    // cascade our location, and check that it doesn't go off screen
                    var newLocation = new Point(openerLocation.X + offset, openerLocation.Y + offset);
                    if (targetScreen.WorkingArea.Contains(new Rectangle(newLocation, formSize)))
                    {
                        return newLocation;
                    }

                    // roll window over
                    return FixUpLocation(targetScreen, newLocation, formSize);
                }
            }

            // else, we are going to try the setting from last closed post
            var lastClosedLocation = PostEditorSettings.PostEditorWindowLocation;
            if (null != FindTitlebarVisibleScreen(lastClosedLocation, formSize, offset))
            {
                return lastClosedLocation;
            }

            // nothing works, return empty point
            return Point.Empty;
        }

        private static Point FixUpLocation(Screen ourScreen, Point location, Size recSize)
        {
            var topRight = new Point(location.X + recSize.Width, location.Y);
            var lowerLeft = new Point(location.X, location.Y + recSize.Height);
            var newLeft = location.X;
            var newTop = location.Y;
            if (!ourScreen.WorkingArea.Contains(topRight))
            {
                newLeft = ourScreen.WorkingArea.X;
            }

            if (!ourScreen.WorkingArea.Contains(lowerLeft))
            {
                newTop = ourScreen.WorkingArea.Y;
            }

            return new Point(newLeft, newTop);
        }

        private static Screen FindVisibleScreen(Point location, Size recSize)
        {
            var allScreens = Screen.AllScreens;
            var rect = new Rectangle(location, recSize);
            foreach (var screen in allScreens)
            {
                if (screen.WorkingArea.IntersectsWith(rect))
                {
                    return screen;
                }
            }

            return null;
        }

        // verifies that if the form is at this location at least the toolbar will be visible
        // helps in cases where screens have been disabled, resolution changed, etc.
        private static Screen FindTitlebarVisibleScreen(Point location, Size formSize, int offset)
        {
            var topbarSize = new Size(formSize.Width, offset);
            return FindVisibleScreen(location, topbarSize);
        }
    }
}
