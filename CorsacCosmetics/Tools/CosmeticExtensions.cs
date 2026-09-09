using CorsacCosmetics.Tools;

public static class CosmeticExtensions
{
    extension(PreviewViewData previewData)
    {
        public void Unload()
        {
            previewData.PreviewSprite.CleanUp();
            previewData.PreviewSprite = null;
            previewData.DestroyImmediate();
        }
    }

    extension(HatViewData hatViewData)
    {
        public void Unload()
        {
            hatViewData.MainImage.CleanUp();
            hatViewData.MainImage = null;
            hatViewData.BackImage.CleanUp();
            hatViewData.BackImage = null;
            hatViewData.ClimbImage.CleanUp();
            hatViewData.ClimbImage = null;
            hatViewData.FloorImage.CleanUp();
            hatViewData.FloorImage = null;
            hatViewData.LeftMainImage.CleanUp();
            hatViewData.LeftMainImage = null;
            hatViewData.LeftBackImage.CleanUp();
            hatViewData.LeftBackImage = null;
            hatViewData.LeftClimbImage.CleanUp();
            hatViewData.LeftClimbImage = null;
            hatViewData.LeftFloorImage.CleanUp();
            hatViewData.LeftFloorImage = null;
            hatViewData.DestroyImmediate();
        }
    }

    extension(VisorViewData visorViewData)
    {
        public void Unload()
        {
            visorViewData.IdleFrame.CleanUp();
            visorViewData.IdleFrame = null;
            visorViewData.LeftIdleFrame.CleanUp();
            visorViewData.LeftIdleFrame = null;
            visorViewData.FloorFrame.CleanUp();
            visorViewData.FloorFrame = null;
            visorViewData.ClimbFrame.CleanUp();
            visorViewData.ClimbFrame = null;
            visorViewData.DestroyImmediate();
        }
    }

    extension(NamePlateViewData nameplateViewData)
    {
        public void Unload()
        {
            nameplateViewData.Image.CleanUp();
            nameplateViewData.Image = null;
            nameplateViewData.DestroyImmediate();
        }
    }
}