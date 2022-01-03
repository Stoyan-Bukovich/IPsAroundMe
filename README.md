# IPs around me
Identifies public live IP addresses around you and shows the distance from you in kilometers and hops.

How to run:

From your operating system terminal / cmd / powershell run ./ipsaroundme for full options list.

-s switch to scan all CIDR  /24 IP addresses around your public IP address.
-r startIP endIP switch to scan IP range.
-c IP/CIDR switch to scan IP range defined by Classless Inter-Domain Routing.


Samle outputs:

**./ipsaroudme -s**


**./ipsaroundme -r 123.123.123.123 123.123.124.0**



**./ipsaroundme -c 123.123.123.123/30**




**Note:** This is initial commit. Needs code refractory and hops time optimization. Any contributions are welcome ;)
