# Platformă Export Marketplace

O aplicație Angular completă pentru gestionarea și exportul produselor către marketplace-uri (eMAG, Altex, Okazii, OLX).

## 🚀 Funcționalități

### 📊 Modul 1: Feed Management
- Încărcare fișiere CSV, Excel, XML, JSON
- Detectare automată a formatului fișierului
- Tabel cu produse cu sortare, filtrare și căutare
- Editare produse prin modal dialog
- Scroll vertical pentru multe produse

### 🗂️ Modul 2: Mapare Categorii & Atribute
- Două panouri tree view pentru categorii
- Drag & drop pentru mapare categorii
- Sugestii AI pentru categorii nemapate
- Form de completare atribute cu auto-sugestii
- Mapare atribute produs (brand, culoare, mărime)

### 📤 Modul 3: Export Produse
- Selectare produse prin checkbox
- Validare produse înainte de export
- Export către marketplace-uri multiple
- Log de export cu produse reușite/eșuate
- Descărcare fișiere export

### 📈 Modul 4: Dashboard & Automatizare
- Dashboard cu statistici în timp real
- Carduri cu metrici importante
- Configurare export programat
- Istoric exporturi recente
- Grafice pentru tendințe

### ⚙️ Modul 5: Admin Panel
- Gestionare utilizatori și roluri
- Configurare token-uri API
- Setări sistem
- Statistici sistem
- Istoric feed-uri și exporturi

## 🛠️ Tehnologii

- **Angular 20** - Framework principal
- **Angular Material** - Componente UI
- **Bootstrap** - Styling și responsive design
- **TypeScript** - Tipizare statică
- **RxJS** - Programare reactivă
- **Angular Router** - Navigare între pagini
- **Reactive Forms** - Formulare complexe

## 📦 Instalare

1. **Clonează repository-ul**
```bash
git clone <repository-url>
cd Front
```

2. **Instalează dependențele**
```bash
npm install
```

3. **Pornește aplicația**
```bash
npm start
```

4. **Accesează aplicația**
```
http://127.0.0.1:4200
```

## 🏗️ Structura Proiectului

```
src/
├── app/
│   ├── components/
│   │   ├── feed-management/          # Gestionare feed-uri
│   │   ├── category-mapping/         # Mapare categorii
│   │   ├── product-export/           # Export produse
│   │   ├── dashboard/                # Dashboard principal
│   │   ├── admin-panel/              # Panou administrator
│   │   ├── product-edit-dialog/      # Dialog editare produs
│   │   └── export-log/               # Log exporturi
│   ├── services/
│   │   ├── feed.service.ts           # Serviciu feed-uri
│   │   ├── category.service.ts       # Serviciu categorii
│   │   ├── export.service.ts         # Serviciu export
│   │   └── admin.service.ts          # Serviciu admin
│   ├── app.module.ts                 # Modul principal
│   ├── app-routing.module.ts         # Routing
│   └── app.component.*               # Componenta principală
├── styles.css                        # Stiluri globale
└── main.ts                           # Entry point
```

## 🎯 Module și Componente

### Feed Management
- **FeedManagementComponent** - Pagina principală de gestionare feed-uri
- **ProductEditDialogComponent** - Dialog pentru editarea produselor
- Funcționalități: upload fișiere, vizualizare produse, editare, filtrare

### Category Mapping
- **CategoryMappingComponent** - Mapare categorii și atribute
- Funcționalități: drag & drop, sugestii AI, mapare atribute

### Product Export
- **ProductExportComponent** - Export produse către marketplace-uri
- **ExportLogComponent** - Log-uri și erori export
- Funcționalități: selectare produse, validare, export, istoric

### Dashboard
- **DashboardComponent** - Dashboard cu statistici și automatizare
- Funcționalități: metrici, grafice, export programat

### Admin Panel
- **AdminPanelComponent** - Gestionare sistem și utilizatori
- Funcționalități: utilizatori, token-uri API, setări sistem

## 🔧 Servicii

### FeedService
- Gestionare produse din feed-uri
- Upload și parsare fișiere
- Căutare și filtrare produse

### CategoryService
- Gestionare categorii feed și marketplace
- Mapare categorii
- Sugestii AI pentru mapare

### ExportService
- Gestionare exporturi
- Validare produse
- Log-uri și erori

### AdminService
- Gestionare utilizatori
- Configurare token-uri API
- Setări sistem

## 🎨 Design și UX

- **Interfață modernă** cu Material Design
- **Responsive design** pentru toate dispozitivele
- **Navigare intuitivă** cu breadcrumbs și tabs
- **Feedback vizual** pentru acțiuni utilizator
- **Loading states** și progres bars
- **Error handling** cu mesaje clare

## 📱 Responsive Design

Aplicația este complet responsive și funcționează pe:
- Desktop (1920px+)
- Laptop (1366px+)
- Tablet (768px+)
- Mobile (320px+)

## 🔒 Securitate

- Validare formulare pe client
- Sanitizare date
- Gestionare securizată token-uri API
- Roluri și permisiuni utilizatori

## 🚀 Deployment

### Build pentru producție
```bash
npm run build
```

### Servire statică
```bash
npx http-server dist/front/browser
```

## 📝 Note

- Aplicația folosește date mock pentru demonstrație
- Toate API-urile sunt simulate cu delay-uri
- Exporturile sunt simulate (nu se fac request-uri reale)
- Sugestiile AI sunt simulate cu date statice

## 🤝 Contribuții

1. Fork repository-ul
2. Creează un branch pentru feature (`git checkout -b feature/AmazingFeature`)
3. Commit schimbările (`git commit -m 'Add some AmazingFeature'`)
4. Push la branch (`git push origin feature/AmazingFeature`)
5. Deschide un Pull Request

## 📄 Licență

Acest proiect este licențiat sub MIT License.
