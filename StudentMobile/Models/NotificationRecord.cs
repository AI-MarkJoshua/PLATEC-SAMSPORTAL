using System.ComponentModel;

namespace StudentMobile.Models
{
    public class NotificationRecord : INotifyPropertyChanged
    {
        public int Id { get; set; }
        
        public int StudentId { get; set; }
        
        public string Title { get; set; } = string.Empty;
        
        public string Message { get; set; } = string.Empty;
        
        public string Type { get; set; } = string.Empty; // "Absent", "Late", "Present"
        
        public DateTime CreatedAt { get; set; }
        
        public bool IsRead { get; set; }
        
        public string DateDisplay => CreatedAt.ToString("MMM dd, yyyy HH:mm");
        
        public string Icon => Type.ToLower() switch
        {
            "absent" => "absent_icon.png",
            "late" => "late_icon.png",
            "present" => "present_icon.png",
            _ => "notification_icon.png"
        };
        
        public string Color => Type.ToLower() switch
        {
            "absent" => "#F44336",
            "late" => "#FF9800",
            "present" => "#4CAF50",
            _ => "#2196F3"
        };

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public void MarkAsRead()
        {
            if (!IsRead)
            {
                IsRead = true;
                OnPropertyChanged(nameof(IsRead));
            }
        }
    }
}
