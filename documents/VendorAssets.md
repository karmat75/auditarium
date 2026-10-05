# Frontend vendor assets

The files under `UI/Auditarium.Web/wwwroot/vendor/` are locally served runtime
assets. They are copied unchanged from the official distribution listed below;
Auditarium-specific CSS and JavaScript are intentionally kept outside this
directory.

| Product | Version | Official source | License | Locally changed |
| --- | ---: | --- | --- | --- |
| Bootstrap | 5.3.8 | [GitHub release distribution](https://github.com/twbs/bootstrap/releases/download/v5.3.8/bootstrap-5.3.8-dist.zip) | MIT | No |
| AdminLTE | 4.9.1 | [Official npm package](https://registry.npmjs.org/admin-lte/-/admin-lte-4.9.1.tgz) | MIT | No |

Only the production CSS and JavaScript required by Auditarium, their source
maps, and the AdminLTE license text are included. No AdminLTE demo assets or
plugins are shipped. Bootstrap's bundled JavaScript contains the Popper runtime
required by its public interactive components, so no separate Popper artifact
is necessary.

## Integrity manifest

The following SHA-256 values document the exact copied files. Recalculate them
after every deliberate vendor upgrade; a mismatch indicates that a vendor file
has changed locally.

| File | SHA-256 |
| --- | --- |
| `vendor/bootstrap/5.3.8/css/bootstrap.min.css` | `D85327D99C7A3EE1F9B5D0500D1370ACEA3AD2DB39C163C2F51F232BAEDBDEDE` |
| `vendor/bootstrap/5.3.8/css/bootstrap.min.css.map` | `48144FAF6AA0FB3CD2CE748D9730238F888F4AB715F05DABD1C9AF2C5671988A` |
| `vendor/bootstrap/5.3.8/js/bootstrap.bundle.min.js` | `E4FD49181388C48EC5040BD3FE66F57C29C8E67FCD8502B3354B96EC7AB47CC7` |
| `vendor/bootstrap/5.3.8/js/bootstrap.bundle.min.js.map` | `C61123E58CC0A4B65D737BA070C485911B3DBEC6D7B802BDF6628395ABD9C08B` |
| `vendor/adminlte/4.9.1/css/adminlte.min.css` | `0934E1E6298DD4666440703DD613FDBB1A3C5F1554467923C29464EF4A5682EA` |
| `vendor/adminlte/4.9.1/css/adminlte.min.css.map` | `CA4230624BD8C5F21D0AACF4728EE712CB87B6A3E206E37CC2AF6286DFC5F667` |
| `vendor/adminlte/4.9.1/js/adminlte.min.js` | `922181E2098AF61F2C6D8E997B1ACCCF37BDFA27C9DD0C9E20D15282AE31EDDB` |
| `vendor/adminlte/4.9.1/js/adminlte.min.js.map` | `001ED020495F79FC57274992E0E26E21BFEB00DC214DFD9109C93A243F325CA8` |
| `vendor/adminlte/4.9.1/LICENSE` | `9E8B0D4E7CE8C13E67A2373D5094910C263FD95B1E49BDF45DC2A9813BE5996B` |
