# IPs around me
Identifies public live IP addresses around you and shows the distance from you in kilometers and hops.

How to run:

From your operating system terminal / cmd / powershell run ./ipsaroundme for full options list.

-s switch to scan all CIDR  /24 IP addresses around your public IP address.
-r startIP endIP switch to scan IP range.
-c IP/CIDR switch to scan IP range defined by Classless Inter-Domain Routing.


Samle outputs:

**./ipsaroudme -s**

Remote IP, Local IP, Distance (km), Hops
----------------------------------------

123.123.123.4,123.123.123.6,32.859,2
123.123.123.5,123.123.123.6,1648.859,4

Your public IP address is: 123.123.123.6
Your default gateway is: 192.168.1.1

Total alive IP addresses: 2
Total dead IP addresses: 254

Finished in: 00:06:44
Started on: 03/01/2022 14:52:55

\
&nbsp;
\
&nbsp;


**./ipsaroundme -r 123.123.123.123 123.123.124.0**

123.123.123.4,123.123.123.6,32.859,2
123.123.123.5,123.123.123.6,1648.859,4

Your public IP address is: 123.123.123.6
Your default gateway is: 192.168.1.1

Total alive IP addresses: 2
Total dead IP addresses: 254

Finished in: 00:06:44
Started on: 03/01/2022 14:52:55

\
&nbsp;
\
&nbsp;

**./ipsaroundme -c 123.123.123.123/30**

123.123.123.4,123.123.123.6,5.859,1
123.123.123.5,123.123.123.6,1648.859,4

Your public IP address is: 123.123.123.6
Your default gateway is: 192.168.1.1

Total alive IP addresses: 2
Total dead IP addresses: 2

Finished in: 00:06:44
Started on: 03/01/2022 14:52:55

\
&nbsp;
\
&nbsp;


**Note:** This is initial commit. Needs code refractory and hops time optimization. Any contributions are welcome ;)
