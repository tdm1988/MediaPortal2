#region Copyright (C) 2007-2021 Team MediaPortal

/*
    Copyright (C) 2007-2021 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using SharpDX;
using SharpDX.Direct3D9;

namespace MediaPortal.UI.SkinEngine.DirectX.RenderPipelines
{
  /// <summary>
  /// Abstract render pipeline that implementes a generic 1-pass 2D rendering.
  /// </summary>
  internal abstract class AbstractRenderPipeline : IRenderPipeline
  {
    public virtual void BeginRender()
    {
      GraphicsDevice.RenderPass = RenderPassType.SingleOrFirstPass;
      GraphicsDevice.Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
      GraphicsDevice.Device.BeginScene();
    }

    public virtual void Render()
    {
      GraphicsDevice.ScreenManager.Render();
    }

    public virtual void EndRender()
    {
      GraphicsDevice.Device.EndScene();
    }

    public virtual void GetVideoClip (RectangleF fullVideoClip, out RectangleF tranformedRect)
    {
      tranformedRect = fullVideoClip;
    }

    public virtual Matrix GetRenderPassTransform (Matrix initialScreenTransform)
    {
      return initialScreenTransform;
    }
  }
}
