import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Sidebar } from '../components/Sidebar'
import { api } from "../api/fetchClient.js"
import { WorkspaceProvider } from '@/utils/WorkspaceContext'
import { Workspace } from './Workspace.jsx'

export function Main() {

  return (
    <div className='main-container'>
      Welcome
    </div>
  )
}