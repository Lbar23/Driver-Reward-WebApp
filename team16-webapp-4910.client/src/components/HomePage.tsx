import React from 'react';
import '../pages/Home.css';  // Import CSS styles for your component

const Home = () => {
  return (
    <div className="home-container">
      {/* Navigation Bar */}
      <nav className="navbar">
        <h1 className="logo">My Website</h1>
        <ul className="nav-links">
          <li><a href="/login">Login</a></li>
          <li><a href="/register">Register</a></li>
        </ul>
      </nav>

      {/* Hero Section */}
      <section className="hero">
        <h2>Welcome to My Website</h2>
        <p>Explore amazing features and enjoy a seamless experience.</p>
        <a href="/register" className="cta-button">Get Started</a>
      </section>

      {/* Footer */}
      <footer className="footer">
        <p>© 2024 My Website. All rights reserved.</p>
      </footer>
    </div>
  );
};

export default Home;
