using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace NameCheapDNSUpdate
{
    public class PostResponse
	{
        
		[XmlRoot(ElementName = "DomainDNSSetHostsResult", Namespace = "http://api.namecheap.com/xml.response")]
		public class DomainDNSSetHostsResult
		{
			[XmlElement(ElementName = "Warnings", Namespace = "http://api.namecheap.com/xml.response")]
			public string Warnings { get; set; }
			[XmlAttribute(AttributeName = "Domain")]
			public string Domain { get; set; }
			[XmlAttribute(AttributeName = "IsSuccess")]
			public string IsSuccess { get; set; }
		}

		[XmlRoot(ElementName = "CommandResponse", Namespace = "http://api.namecheap.com/xml.response")]
		public class CommandResponse
		{
			[XmlElement(ElementName = "DomainDNSSetHostsResult", Namespace = "http://api.namecheap.com/xml.response")]
			public DomainDNSSetHostsResult DomainDNSSetHostsResult { get; set; }
			[XmlAttribute(AttributeName = "Type")]
			public string Type { get; set; }
		}

		[XmlRoot(ElementName = "ApiResponse", Namespace = "http://api.namecheap.com/xml.response")]
		public class ApiPostResponse
		{
			[XmlElement(ElementName = "Errors", Namespace = "http://api.namecheap.com/xml.response")]
			public string Errors { get; set; }
			[XmlElement(ElementName = "Warnings", Namespace = "http://api.namecheap.com/xml.response")]
			public string Warnings { get; set; }
			[XmlElement(ElementName = "RequestedCommand", Namespace = "http://api.namecheap.com/xml.response")]
			public string RequestedCommand { get; set; }
			[XmlElement(ElementName = "CommandResponse", Namespace = "http://api.namecheap.com/xml.response")]
			public CommandResponse CommandResponse { get; set; }
			[XmlElement(ElementName = "Server", Namespace = "http://api.namecheap.com/xml.response")]
			public string Server { get; set; }
			[XmlElement(ElementName = "GMTTimeDifference", Namespace = "http://api.namecheap.com/xml.response")]
			public string GMTTimeDifference { get; set; }
			[XmlElement(ElementName = "ExecutionTime", Namespace = "http://api.namecheap.com/xml.response")]
			public string ExecutionTime { get; set; }
			[XmlAttribute(AttributeName = "Status")]
			public string Status { get; set; }
			[XmlAttribute(AttributeName = "xmlns")]
			public string Xmlns { get; set; }
		}
	}
}
