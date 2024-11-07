import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter } from "react-router-dom"
import App from './App'
//import './index.css'
// import reportWebVitals from './reportWebVitals'

ReactDOM.createRoot(document.getElementById('root')!).render(
    // <React.StrictMode>
        <BrowserRouter>
            <App />
        </BrowserRouter>,
    // </React.StrictMode>, // don't use this b/c it duplicates api calls and adds too much strain on servers
)
