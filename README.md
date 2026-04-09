USE [JSL_HSM_DB]
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_Delete_Slab]
(
    @SlabID    VARCHAR(20),
    @Code      INT OUTPUT,
    @Msg       VARCHAR(200) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET @Code = 500;
    SET @Msg  = 'Unknown error';

    -- ── Validation ────────────────────────────────────────────────────────────
    IF NULLIF(LTRIM(RTRIM(@SlabID)), '') IS NULL
    BEGIN
        SET @Code = 400;
        SET @Msg  = 'SlabID cannot be empty';
        RETURN;
    END

    -- ── Check if slab exists ──────────────────────────────────────────────────
    IF NOT EXISTS (SELECT 1 FROM dbo.Slab_Upload_Masters WHERE SLABID = @SlabID)
    AND NOT EXISTS (SELECT 1 FROM dbo.SLAB_LMS_DETAILS WHERE SLABID = @SlabID)
    BEGIN
        SET @Code = 404;
        SET @Msg  = 'SlabID not found';
        RETURN;
    END

    -- ── Delete from all tables in correct order ───────────────────────────────
    BEGIN TRY
        BEGIN TRANSACTION;

            -- 1. PikeStoresData
            DELETE FROM dbo.SLAB_LMS_DETAILS_PikeStoresData
            WHERE SLABID = @SlabID;

            -- 2. LMS Details
            DELETE FROM dbo.SLAB_LMS_DETAILS
            WHERE SLABID = @SlabID;

            -- 3. LMS Details L2
            DELETE FROM dbo.SLAB_LMS_DETAILS_L2
            WHERE SLABID = @SlabID;

            -- 4. Slab Upload Master L2
            DELETE FROM dbo.Slab_Upload_Master_L2
            WHERE SLABID = @SlabID;

            -- 5. Slab Upload Masters
            DELETE FROM dbo.Slab_Upload_Masters
            WHERE SLABID = @SlabID;

        COMMIT TRANSACTION;

        SET @Code = 200;
        SET @Msg  = 'Slab deleted successfully';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SET @Code = 500;
        SET @Msg  = ERROR_MESSAGE();
    END CATCH

END
GO
