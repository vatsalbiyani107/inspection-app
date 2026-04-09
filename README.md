USE [JSL_HSM_DB]
GO
/****** Object:  StoredProcedure [dbo].[usp_DeleteSlabsByIds]    Script Date: 09-04-2026 15:30:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_DeleteSlabsByIds]
    @SlabIds NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Slab_Upload_Masters
    WHERE SLABID IN (SELECT TRIM(value) FROM STRING_SPLIT(@SlabIds, ','));

    DELETE FROM dbo.SLAB_LMS_DETAILS_PikeStoresData
    WHERE SLABID IN (SELECT TRIM(value) FROM STRING_SPLIT(@SlabIds, ','));

	DELETE FROM [dbo].[Slab_Upload_Master_L2]
    WHERE SLABID IN (SELECT TRIM(value) FROM STRING_SPLIT(@SlabIds, ','));

	DELETE FROM [dbo].[SLAB_LMS_DETAILS_L2]
    WHERE SLABID IN (SELECT TRIM(value) FROM STRING_SPLIT(@SlabIds, ','));

	DELETE FROM [dbo].[SLAB_LMS_DETAILS]
    WHERE SLABID IN (SELECT TRIM(value) FROM STRING_SPLIT(@SlabIds, ','));

END
