# IPs around me
Identifies public live IP addresses around you and shows the distance from you in kilometers and hops.

How to run:

From your operating system terminal / cmd / powershell run ./ipsaroundme for full options list.

-s switch to scan all CIDR  /24 IP addresses around your public IP address.
\
&nbsp;
-r startIP endIP switch to scan IP range.
\
&nbsp;
-c IP/CIDR switch to scan IP range defined by Classless Inter-Domain Routing.
\
&nbsp;

Samle outputs:

**./ipsaroudme -s**

Remote IP, Local IP, Distance (km), Hops

\
&nbsp;
123.123.123.4,123.123.123.6,32.859,2
\
&nbsp;
123.123.123.5,123.123.123.6,1648.859,4
\
&nbsp;
\
&nbsp;
Your public IP address is: 123.123.123.6
\
&nbsp;
Your default gateway is: 192.168.1.1
\
&nbsp;
\
&nbsp;
Total alive IP addresses: 2
\
&nbsp;
Total dead IP addresses: 254
\
&nbsp;
\
&nbsp;
Finished in: 00:06:44
\
&nbsp;
Started on: 03/01/2022 14:52:55

\
&nbsp;
\
&nbsp;


**./ipsaroundme -r 123.123.123.123 123.123.124.0**

Remote IP, Local IP, Distance (km), Hops

\
&nbsp;
123.123.123.4,123.123.123.6,32.859,2
\
&nbsp;
123.123.123.5,123.123.123.6,1648.859,4
\
&nbsp;
\
&nbsp;
Your public IP address is: 123.123.123.6
\
&nbsp;
Your default gateway is: 192.168.1.1
\
&nbsp;
\
&nbsp;
Total alive IP addresses: 2
\
&nbsp;
Total dead IP addresses: 254
\
&nbsp;
\
&nbsp;
Finished in: 00:06:44
\
&nbsp;
Started on: 03/01/2022 14:52:55

\
&nbsp;
\
&nbsp;

**./ipsaroundme -c 123.123.123.123/30**

Remote IP, Local IP, Distance (km), Hops

\
&nbsp;
123.123.123.4,123.123.123.6,5.859,1
\
&nbsp;
123.123.123.5,123.123.123.6,1648.859,4
\
&nbsp;
\
&nbsp;
Your public IP address is: 123.123.123.6
\
&nbsp;
Your default gateway is: 192.168.1.1
\
&nbsp;
\
&nbsp;
Total alive IP addresses: 2
\
&nbsp;
Total dead IP addresses: 2
\
&nbsp;
\
&nbsp;
Finished in: 00:06:44
\
&nbsp;
Started on: 03/01/2022 14:52:55

\
&nbsp;
\
&nbsp;


**Note:** This is initial commit. Needs code refractory and hops time optimization. Any contributions are welcome ;)
