# Frontend vendor assets

The files under `UI/Auditarium.Web/wwwroot/vendor/` are locally served runtime
assets. They are copied unchanged from the official distribution listed below;
Auditarium-specific CSS and JavaScript are intentionally kept outside this
directory.

| Product | Version | Official source | License | Locally changed |
| --- | ---: | --- | --- | --- |
| Bootstrap | 5.3.8 | [GitHub release distribution](https://github.com/twbs/bootstrap/releases/download/v5.3.8/bootstrap-5.3.8-dist.zip) | MIT | No |
| Bootstrap Icons | 1.13.1 | [GitHub release distribution](https://github.com/twbs/icons/releases/download/v1.13.1/bootstrap-icons-1.13.1.zip) | MIT | No |
| AdminLTE | 4.9.1 | [Official npm package](https://registry.npmjs.org/admin-lte/-/admin-lte-4.9.1.tgz) | MIT | No |
| Tabulator | 6.5.0 | [Official npm package](https://registry.npmjs.org/tabulator-tables/-/tabulator-tables-6.5.0.tgz) | MIT | No |

Only the production CSS, JavaScript, and icon fonts required by Auditarium,
their source maps where applicable, and the applicable license texts are
included. No AdminLTE demo assets or plugins are shipped. Bootstrap's bundled
JavaScript contains the Popper runtime required by its public interactive
components, so no separate Popper artifact is necessary.

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
| `vendor/bootstrap-icons/1.13.1/bootstrap-icons.min.css` | `A5D6387A32CA3BAEC4D02336B5B3EDAB50C9DD518355576A011EA3DD9C1D884E` |
| `vendor/bootstrap-icons/1.13.1/fonts/bootstrap-icons.woff` | `F55513B7B591CB84A3B87FF0E34EA24D4831D6FEDC22E54B911CA64B5B544A15` |
| `vendor/bootstrap-icons/1.13.1/fonts/bootstrap-icons.woff2` | `6C75710364A1CA5604267716F6D28997B26319FDB078CF11E0B42AB66FF2EA61` |
| `vendor/bootstrap-icons/1.13.1/LICENSE` | `0FB3E11BD57E896C5A512AFD64864D28A37DE45D19835016C87CA1AD19EAD969` |
| `vendor/adminlte/4.9.1/css/adminlte.min.css` | `0934E1E6298DD4666440703DD613FDBB1A3C5F1554467923C29464EF4A5682EA` |
| `vendor/adminlte/4.9.1/css/adminlte.min.css.map` | `CA4230624BD8C5F21D0AACF4728EE712CB87B6A3E206E37CC2AF6286DFC5F667` |
| `vendor/adminlte/4.9.1/js/adminlte.min.js` | `922181E2098AF61F2C6D8E997B1ACCCF37BDFA27C9DD0C9E20D15282AE31EDDB` |
| `vendor/adminlte/4.9.1/js/adminlte.min.js.map` | `001ED020495F79FC57274992E0E26E21BFEB00DC214DFD9109C93A243F325CA8` |
| `vendor/adminlte/4.9.1/LICENSE` | `9E8B0D4E7CE8C13E67A2373D5094910C263FD95B1E49BDF45DC2A9813BE5996B` |
| `vendor/tabulator/6.5.0/tabulator_bootstrap5.min.css` | `46F2E6AFD39E51167B1C850B20F7F1DA608495E5EC293CE0826FCA4E6A36CACE` |
| `vendor/tabulator/6.5.0/tabulator.min.js` | `297F32DE082FE4593D385F3A1B16B901F5208B1702FF9F64F454F3460D911170` |
| `vendor/tabulator/6.5.0/LICENSE` | `191A2EE554684E1064C897B432F0E1BC6DFA714CA045D3F6EA2CF692CBD398B7` |
