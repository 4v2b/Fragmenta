import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import basicSsl from '@vitejs/plugin-basic-ssl'
import path from "path";

// https://vite.dev/config/
export default defineConfig({
  server: {
    host: '0.0.0.0',
    port: 5173, // or any available port
  },
  plugins: [
    react(), 
    //basicSsl()
  ],
  resolve: {
    alias: {
        "@": path.resolve(__dirname, "src"), // Define "@" as an alias for "src"
    },
},
})
