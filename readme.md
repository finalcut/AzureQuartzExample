For some information on how this project is organized refer to this blog post:
http://code.rawlinson.us/2016/04/autofac.html

For information on the deployment via TeamCity check out this blog post:
http://code.rawlinson.us/2016/04/deploying-azure-worker-role-via-team-city.html

note: if you add new settings to the role you need to manually edit the `ServiceConfiguration.Cloud.cscfg` file on the TeamCity server
`C:\Agent1\work\1daf6f8d35186a85\Aemis.Scheduler`

new people working on the project need to copy the `ServiceConfiguration.Template.cscfg` file to `ServiceConfiguration.local.cscfg` and provide the various settings (secret values) that are available
in https://drive.google.com/open?id=0B_yFuhOKqc0GMGt2bmZ4VzQ2OVU  which is a copy of the `ServiceConfiguration.Cloud.cscfg` file

