namespace ASC.Models.BaseTypes
{
    public static class Constants
    {
        public enum Roles
        {
            Admin, Engineer, User
        }
        public enum Masterkeys
        {
            VehicleName, VehicleType
        }
        public enum Status
        {
            New, Denied, Pending, Initiated, InProgress, PendingCustomerApproval,
            RequestForInformation, Completed
        }
    }
}