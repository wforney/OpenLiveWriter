// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    /// <summary>
    /// Class WindowCascadeHelper. This class cannot be inherited.
    /// </summary>
    public static class WindowCascadeHelper
    {
        /// <summary>
        /// The reference location
        /// </summary>
        private static Point referenceLocation;

        /// <summary>
        /// Sets the next opened location.
        /// </summary>
        /// <param name="location">The location.</param>
        public static void SetNextOpenedLocation(Point location) => WindowCascadeHelper.referenceLocation = location;

        /// <summary>
        /// Gets the new post location.
        /// </summary>
        /// <param name="formSize">Size of the form.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>Point.</returns>
        public static Point GetNewPostLocation(Size formSize, int offset)
        {
            // case 1: opened through new post, so there is a window to cascade against
            if (!WindowCascadeHelper.referenceLocation.IsEmpty)
            {
                var openerLocation = WindowCascadeHelper.referenceLocation;

                // make sure that we will cascade against a visible window
                var targetScreen = WindowCascadeHelper.FindTitlebarVisibleScreen(openerLocation, formSize, offset);
                if (null != targetScreen)
                {
                    // cascade our location, and check that it doesn't go off screen
                    var newLocation = new Point(openerLocation.X + offset, openerLocation.Y + offset);
                    if (targetScreen.WorkingArea.Contains(new Rectangle(newLocation, formSize)))
                    {
                        return newLocation;
                    }

                    // roll window over
                    return WindowCascadeHelper.FixUpLocation(targetScreen, newLocation, formSize);
                }
            }

            // else, we are going to try the setting from last closed post
            var lastClosedLocation = PostEditorSettings.PostEditorWindowLocation;
            if (WindowCascadeHelper.FindTitlebarVisibleScreen(lastClosedLocation, formSize, offset) == null)
            {
                // nothing works, return empty point
                return Point.Empty;
            }

            return lastClosedLocation;
        }

        /// <summary>
        /// Fixes up location.
        /// </summary>
        /// <param name="ourScreen">Our screen.</param>
        /// <param name="location">The location.</param>
        /// <param name="recSize">Size of the record.</param>
        /// <returns>Point.</returns>
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

        /// <summary>
        /// Finds the visible screen.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="recSize">Size of the record.</param>
        /// <returns>Screen.</returns>
        private static Screen FindVisibleScreen(Point location, Size recSize) =>
            Screen.AllScreens.FirstOrDefault(
                screen => screen.WorkingArea.IntersectsWith(new Rectangle(location, recSize)));

        /// <summary>
        /// verifies that if the form is at this location at least the toolbar will be visible
        /// helps in cases where screens have been disabled, resolution changed, etc.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="formSize">Size of the form.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>Screen.</returns>
        private static Screen FindTitlebarVisibleScreen(Point location, Size formSize, int offset) =>
            WindowCascadeHelper.FindVisibleScreen(location, new Size(formSize.Width, offset));
    }
}
