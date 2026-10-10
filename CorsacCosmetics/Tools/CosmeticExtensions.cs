namespace CorsacCosmetics.Tools;

public static class CosmeticExtensions
{
    extension(PreviewViewData previewData)
    {
        public void Release()
        {
            previewData.PreviewSprite = null;
            previewData.DestroyImmediate();
        }
    }

    extension(HatViewData hatViewData)
    {
        public void Release()
        {
            hatViewData.MainImage = null;
            hatViewData.BackImage = null;
            hatViewData.ClimbImage = null;
            hatViewData.FloorImage = null;
            hatViewData.LeftMainImage = null;
            hatViewData.LeftBackImage = null;
            hatViewData.LeftClimbImage = null;
            hatViewData.LeftFloorImage = null;
            hatViewData.DestroyImmediate();
        }
    }

    extension(VisorViewData visorViewData)
    {
        public void Release()
        {
            visorViewData.IdleFrame = null;
            visorViewData.LeftIdleFrame = null;
            visorViewData.FloorFrame = null;
            visorViewData.ClimbFrame = null;
            visorViewData.DestroyImmediate();
        }
    }

    extension(NamePlateViewData nameplateViewData)
    {
        public void Release()
        {
            nameplateViewData.Image = null;
            nameplateViewData.DestroyImmediate();
        }
    }
}