import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import { Login } from './pages/Login/Login.jsx'
import './i18n';
import { BrowserRouter, Routes, Route } from "react-router";
import { Layout } from './components/Layout/Layout.jsx'
import { Register } from './pages/Register/Register.jsx'
import { AuthLayout } from './components/AuthLayout/AuthLayout.jsx'
import { Main } from './pages/Main/Main.jsx'
import { ChakraProvider } from "@chakra-ui/react"
import { system } from "@chakra-ui/react/preset";


createRoot(document.getElementById('root')).render(
  <StrictMode>
    <ChakraProvider value={system}>
      <BrowserRouter>
        <Routes>
          <Route element={<Layout />}>
            <Route path="login" element={<Login />} />
            <Route path="register" element={<Register />} />
          </Route>

          <Route element={<AuthLayout />}>
            <Route index element={<Main />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </ChakraProvider>
  </StrictMode>
)
