using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenArchitect.Events
{
    public record EventReceived(string Message) : INotification;

}
