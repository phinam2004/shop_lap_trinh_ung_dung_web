namespace SV22T1020673.Admin
{
    /// <summary>
    /// Lớp biểu diễn kết quả khi gọi api
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public ApiResult(int code, string messenge) {
            Code = code;
            Message = messenge;
        }
        
        /// <summary>
        /// 0: Lỗi/hoặc không thành công , lớn hơn 0: Thành công
        /// </summary>
        public int Code { get; set; }
        public string Message { get; set; }
    }
}
