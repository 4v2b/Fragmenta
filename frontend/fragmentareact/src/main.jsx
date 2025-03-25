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
import { Workspace } from './pages/Workspace/Workspace';
import { Board } from './pages/Board';
import { MainLayout } from './components/MainLayout';
import { TagsProvider } from './utils/TagContext';


createRoot(document.getElementById('root')).render(
  <StrictMode>
    <ChakraProvider resetCSS={false} value={system}>
      <BrowserRouter>
        <Routes>
          <Route element={<Layout />}>
            <Route path="login" element={<Login />} />
            <Route path="register" element={<Register />} />
          </Route>

          <Route element={<AuthLayout />}>
            <Route element={<MainLayout />}>
              <Route index element={<Main />}></Route>

              <Route path="workspaces/:workspaceId" element={<Workspace />}>

              </Route>
              <Route path="workspaces/:workspaceId/boards/:boardId" element={
                <TagsProvider>
                  <Board />
                </TagsProvider>
              }>
              </Route>

            </Route>
          </Route>

        </Routes>
      </BrowserRouter>
    </ChakraProvider>
  </StrictMode>
)
