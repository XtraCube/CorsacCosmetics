using CorsacCosmetics.Tools;

public static class CosmeticExtensions
{
    extension(PreviewViewData previewData)
    {
        public void Unload()
        {
            previewData.PreviewSprite.DestroyImmediate();
            previewData.PreviewSprite = null;
        }
    }

    extension(HatViewData hatViewData)
    {
        public void Unload()
        {
            hatViewData.MainImage.DestroyImmediate();
            hatViewData.MainImage = null;
            hatViewData.BackImage.DestroyImmediate();
            hatViewData.BackImage = null;
            hatViewData.ClimbImage.DestroyImmediate();
            hatViewData.ClimbImage = null;
            hatViewData.FloorImage.DestroyImmediate();
            hatViewData.FloorImage = null;
            hatViewData.LeftMainImage.DestroyImmediate();
            hatViewData.LeftMainImage = null;
            hatViewData.LeftBackImage.DestroyImmediate();
            hatViewData.LeftBackImage = null;
            hatViewData.LeftClimbImage.DestroyImmediate();
            hatViewData.LeftClimbImage = null;
            hatViewData.LeftFloorImage.DestroyImmediate();
            hatViewData.LeftFloorImage = null;
        }
    }

    extension(VisorViewData visorViewData)
    {
        public void Unload()
        {
            visorViewData.IdleFrame.DestroyImmediate();
            visorViewData.IdleFrame = null;
            visorViewData.LeftIdleFrame.DestroyImmediate();
            visorViewData.LeftIdleFrame = null;
            visorViewData.FloorFrame.DestroyImmediate();
            visorViewData.FloorFrame = null;
            visorViewData.ClimbFrame.DestroyImmediate();
            visorViewData.ClimbFrame = null;
        }
    }

    extension(NamePlateViewData nameplateViewData)
    {
        public void Unload()
        {
            nameplateViewData.Image.DestroyImmediate();
            nameplateViewData.Image = null;
        }
    }
}