
USE [JSL_HSM_DB]
GO
/****** Object:  Trigger [dbo].[trg_SlabUploadToSAP]    Script Date: 27-02-2026 14:41:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create TRIGGER [dbo].[trg_SlabUploadToSAP]
ON [dbo].[Slab_Upload_Master]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [XX.X.XX.XXX].[XXX_XXX_XX].[dbo].[Slab_Upload_Master] (SlabID, Upload_Date, Status)
    SELECT SlabID, GETDATE(), 0
    FROM INSERTED

END
