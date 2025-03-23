import { useEffect, useState } from 'react'
import './Main.css'
import { useTranslation } from 'react-i18next'
import { Sidebar } from '../../components/Sidebar/Sidebar'
import { api } from "../../api/fetchClient.js"
import { WorkspaceProvider } from '@/utils/WorkspaceContext'
import { Workspace } from '../Workspace/Workspace.jsx'

export function Main() {

  return (
    <div className='main-container'>
      Welcome
    </div>
  )
}