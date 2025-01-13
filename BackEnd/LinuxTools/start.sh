chmod -R o-rwx Recepten
chmod -R g-rwx Recepten
chmod -R o-rwx *.sh
chmod -R g-rwx *.sh
chmod -R o-rwx google-key-file.json
chmod -R g-rwx google-key-file.json
chmod -R u+r-wx google-key-file.json
chmod -R u+rwx Recepten
chmod -R u+rwx *.sh
chown -R www-data:www-data *
chown root:root recepten.service
systemctl start recepten.service