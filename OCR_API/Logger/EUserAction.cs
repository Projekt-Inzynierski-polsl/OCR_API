namespace OCR_API.Logger
{
    public enum EUserAction
    {
        None,
        Registration,
        Login,
        RefreshToken,
        LogoutUser,
        CreateFolder,
        DeleteFolder,
        UpdateFolder,
        LockFolder,
        UnlockFolder,
        CreateNote,
        DeleteNote,
        UpdateNote,
        ChangeNoteFolder,
        UpdateNoteCategories,
        ReportError,
        UpdateUser,
        DeleteUser,
        DeleteError,
        DownloadErrors,
        ClearErrorTable,
        AddCategory,
        DeleteCategory,
        UpdateCategory,
        ShareNote,
        ShareFolder,
        UnshareNote,
        UnshareFolder,
        UploadedFile,
        CryptographyFile,
        AcceptError

    }
}
