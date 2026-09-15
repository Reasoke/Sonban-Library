<template>
  <footer>
    <div class="logo">
      <img class="logo_img" alt="logo" src="../assets/logo.png">
    </div>
    <div class="footer-right">
      <h3>Contacts</h3>
      <p>Adress: Ozz, Eastern Tower 3</p>
      <p>Phone: +223-322</p>
      <p>Email: sonban@email.com</p>
    </div>
  </footer>
</template>

<script>
</script>

<style scoped>
footer{
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  background-color: #d4d4d4;
  padding: 17px 30px;
  flex-wrap: wrap;
  /* gap: 20px; */
}

.logo_img {
  height: 6.5rem;
  font-size: 1.5rem;
  font-weight: bold;
}

.logo_img .logo {
  width: 100px;
  height: auto;
}

.footer-right {
  display: flex;
  flex-direction: column;
  color: #333;
  padding-right: 30px;
}

.footer-right h3 {
  margin-bottom: 10px;
}

.footer-right p {
  margin: 4px 0;
}
</style>
