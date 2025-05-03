import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import { Login } from './pages/Login.jsx'
import './i18n';
import { BrowserRouter, Routes, Route } from "react-router";
import { Layout } from './components/Layout/Layout.jsx'
import { Register } from './pages/Register.jsx'
import { Main } from './pages/Main.jsx'
import { ChakraProvider } from "@chakra-ui/react"
import { Workspace } from './pages/Workspace';
import { Board } from './pages/Board';
import { MainLayout } from '@/components/MainLayout';
import { TagsProvider } from './utils/TagContext';
import { TasksProvider } from './utils/TaskContext';
import { ForgotPassword } from './pages/ForgotPassword';
import { ResetPassword } from './pages/ResetPassword';
import { system } from "./theme"; // Ensure you import your theme
import { Settings } from './pages/Settings';
import { UserProvider } from './utils/UserContext';


createRoot(document.getElementById('root')).render(
  <StrictMode>
    <ChakraProvider resetCSS={false} value={system}>
      <BrowserRouter>
        <Routes>
          <Route element={<Layout />}>
            <Route path="login" element={<Login />} />
            <Route path="register" element={<Register />} />
            <Route path="forgot-password" element={<ForgotPassword />} />
            <Route path="reset-password" element={<ResetPassword />} />
          </Route>

          <Route element={<MainLayout />}>
            
              <Route index element={<Main />} />
              <Route path="workspaces/:workspaceId" element={<Workspace />} />
              <Route path="me" element={<Settings />} />
              <Route path="workspaces/:workspaceId/boards/:boardId" element={
                <TasksProvider>
                  <TagsProvider>
                    <Board />
                  </TagsProvider>
                </TasksProvider>
              }>
              </Route>
          </Route>


        </Routes>
      </BrowserRouter>
    </ChakraProvider>
  </StrictMode>
)
